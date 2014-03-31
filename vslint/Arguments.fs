
module vslint.Arguments

open System
open System.IO

open IO

type Argument =
    | MachineReadable
    | OnlyIssues
    | Path of string

type Arguments(argv : array<string>) =
    let parsedArguments =
        let rec walk (list : list<string>) =
            if list.IsEmpty then
                []
            else
                match list.Head with
                | "-M" | "--machine-readable" -> Argument.MachineReadable :: walk list.Tail
                | "-I" | "--issues-only" -> Argument.OnlyIssues :: walk list.Tail
                | _ -> Argument.Path(list.Head) :: walk list.Tail
        walk (argv |> List.ofArray)

    member this.PrintMachineReadable =
        parsedArguments |> List.exists (fun x -> x = Argument.MachineReadable)

    member this.PrintOnlyIssues =
        parsedArguments |> List.exists (fun x -> x = Argument.OnlyIssues || x = Argument.MachineReadable)

    member this.PathsToExamine =
        let getPath =
            function
            | Path(path) ->
                if isDirectory path then path
                else Path.GetDirectoryName(path)
            | _ -> ""
        parsedArguments
        |> Seq.map getPath
        |> Seq.filter (fun x -> not (String.IsNullOrWhiteSpace x))
        |> Seq.distinct
        |> List.ofSeq

    static member PrintOptions =
        printfn "Options:"
        printfn "-I, --issues-only       Print only when issues are found, otherwise silent"
        printfn "-M, --machine-readable  Print results in machine readable format, implies -I"
