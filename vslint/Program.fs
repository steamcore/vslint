
open System

open vslint.Console
open vslint.IO
open vslint.Ignore
open vslint.ProjectAnalysis
open vslint.Arguments

[<EntryPoint>]
let main argv =

    let args = new Arguments(argv)

    if args.PathsToExamine.IsEmpty then
        printfn "Usage: vslint [options..] path [path2 path3 ..]"
        Arguments.PrintOptions
        0
    else
        let analyze (projectPath : string) =
            analyzeProject projectPath (tryFindSourceControlRoot projectPath)

        let printResults (results : AnalysisResult) =
            if args.PrintMachineReadable then
                results.PrintMachineReadable
            else
                results.PrintHumanReadable args.PrintOnlyIssues
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

        if (not args.PrintMachineReadable && (not args.PrintOnlyIssues || numErrors > 0)) then
            printfn "Found %d issues" numErrors

        if numErrors > 0 then 1
        else 0
