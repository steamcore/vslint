
module vslint.Arguments

open System
open System.IO

open IO

type Argument =
    | Help
    | MachineReadable
    | Verbose
    | Quiet
    | Path of string

type Arguments(argv : array<string>) =
    let parsedArguments =
        let rec walk (list : list<string>) =
            if list.IsEmpty then
                []
            else
                match list.Head with
                | "-h" | "--help" -> Argument.Help :: walk list.Tail
                | "-m" | "--machine-readable" -> Argument.MachineReadable :: walk list.Tail
                | "-v" | "--verbose" -> Argument.Verbose :: walk list.Tail
                | "-q" | "--quiet" -> Argument.Quiet :: walk list.Tail
                | _ -> Argument.Path(list.Head) :: walk list.Tail
        walk (argv |> List.ofArray)

    member this.PrintHelp =
        parsedArguments |> List.exists (fun x -> x = Argument.Help)

    member this.PrintMachineReadable =
        parsedArguments |> List.exists (fun x -> x = Argument.MachineReadable)

    member this.Verbose =
        parsedArguments |> List.exists (fun x -> x = Argument.Verbose)

    member this.Quiet =
        parsedArguments |> List.exists (fun x -> x = Argument.Quiet)

    member this.PathsToExamine =
        let getPath =
            function
            | Path(path) ->
                if Directory.Exists(path) then path
                else if File.Exists(path) then Path.GetDirectoryName(path)
                else ""
            | _ -> ""
        let paths =
            parsedArguments
            |> Seq.map getPath
            |> Seq.filter (fun x -> not (String.IsNullOrWhiteSpace x))
            |> Seq.distinct
            |> List.ofSeq
        if paths.IsEmpty then ["."]
        else paths

    static member PrintOptions =
        printfn "Options:"
        printfn "-h, --help              Prints this help message"
        printfn "-m, --machine-readable  Print results in an alternate machine readable format"
        printfn "-v, --verbose           Lists scanned projects even if no issues are found"
        printfn "-q, --quiet             Quiet unless issues are found"
