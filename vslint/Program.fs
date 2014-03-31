
open System

open vslint.Console
open vslint.IO
open vslint.Ignore
open vslint.ProjectAnalysis
open vslint.Arguments

[<EntryPoint>]
let main argv =

    let args = new Arguments(argv)

    if args.PathsToExamine.IsEmpty || args.PrintHelp then
        printfn "vslint, a tool for detecting inconsistencies in Visual Studio project files"
        printfn "Usage: vslint [options..] path [path2 path3 ..]"
        printfn ""
        Arguments.PrintOptions
        0
    else
        let analyze (projectPath : string) =
            analyzeProject projectPath (tryFindSourceControlRoot projectPath)

        let printResults (results : AnalysisResult) =
            if not args.Quiet then
                if args.PrintMachineReadable then
                    results.PrintMachineReadable
                else
                    results.PrintHumanReadable args.Verbose
            results

        let results =
            args.PathsToExamine
            |> Seq.map findProjectFiles
            |> Seq.concat
            |> Array.ofSeq
            |> Array.Parallel.map analyze
            |> Seq.map printResults
            |> List.ofSeq

        let numErrors =
            results
            |> List.sumBy (fun x -> x.NumberOfIssues)

        if (not args.Quiet && not args.PrintMachineReadable) then
            printfn "Scanned %d projects, found %d issues" results.Length numErrors

        if numErrors > 0 then 1
        else 0
