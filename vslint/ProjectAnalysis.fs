
module vslint.ProjectAnalysis

open System;
open System.IO
open System.Text.RegularExpressions

open IO
open Ignore
open Console
open ProjectParsing

/// <summary>
/// Contains lists of issues detected when analyzing a project file
/// </summary>
type AnalysisResult (projectPath : string, duplicateItems : seq<string>, itemsNotInProject : seq<string>, itemsNotOnDisk : seq<string>) =
    member this.ProjectPath = projectPath
    member this.DuplicateItems = duplicateItems |> List.ofSeq
    member this.ItemsNotInProject = itemsNotInProject |> List.ofSeq
    member this.ItemsNotOnDisk = itemsNotOnDisk |> List.ofSeq
    member this.NumberOfIssues = this.DuplicateItems.Length + this.ItemsNotInProject.Length + this.ItemsNotOnDisk.Length
    member this.IssuesDetected = this.NumberOfIssues > 0
    member this.PrintHumanReadable (verbose : bool) =
        if this.IssuesDetected || verbose then
            printfn "Project %s" projectPath
        if this.IssuesDetected then
            if not (List.isEmpty this.DuplicateItems) then
                printWarning "  References with multiple declarations found:"
                for item in this.DuplicateItems do
                    printWarning "    %s" item
            if not (List.isEmpty this.ItemsNotInProject) then
                printWarning "  Found files on disk that are not referenced in the project:"
                for item in this.ItemsNotInProject do
                    printWarning "    %s" item
            if not (List.isEmpty this.ItemsNotOnDisk) then
                printError "  Found references to files in the project that are missing on disk:"
                for item in this.ItemsNotOnDisk do
                    printError "    %s" item
            printfn ""
        elif verbose then
            printfn "  no issues"
            printfn ""
    member this.PrintMachineReadable (verbose : bool) =
        if this.IssuesDetected || verbose then
            printfn "# %s" projectPath
        if this.IssuesDetected then
            if not (List.isEmpty this.DuplicateItems) then
                for item in this.DuplicateItems do
                    printfn "D %s" item
            if not (List.isEmpty this.ItemsNotInProject) then
                for item in this.ItemsNotInProject do
                    printfn "N %s" item
            if not (List.isEmpty this.ItemsNotOnDisk) then
                for item in this.ItemsNotOnDisk do
                    printfn "M %s" item

/// <summary>
/// Parses a Visual Studio project file, compares it to files on disk and detects discrepancies
/// </summary>
/// <param name="projectPath">Path to project file</param>
/// <param name="sourceControlRoot">Path to source control root, should be the folder any .ignore files are located</param>
/// <param name="patternsToIgnore">List of regular expressions that matches files to be ignored in analysis</param>
let analyzeProject projectPath sourceControlRoot =

    let projectDirectory =
        Path.GetFullPath (Path.GetDirectoryName projectPath)

    let findDuplicates items =
        items
            |> Seq.countBy id
            |> Seq.filter (snd >> ((<) 1))
            |> Seq.map fst

    let fileExists file =
        let targetPath = Path.Combine(projectDirectory, file)
        File.Exists(targetPath)

    let getPathRelativeToSourceControlRoot itemPath =
        Path.Combine(projectDirectory, itemPath).Replace(sourceControlRoot, "").TrimStart('\\')

    let shouldIgnore =
        let ignoreExpression =
            let patternSet = getIgnorePatterns projectPath |> List.sort |> Set.ofList
            new Regex(String.Join("|", patternSet), RegexOptions.Singleline ||| RegexOptions.IgnoreCase)
        fun path -> ignoreExpression.IsMatch <| getPathRelativeToSourceControlRoot path
    
    let filterIgnoredResultsInParallel (results : seq<string>) =
        results
        |> Array.ofSeq
        |> Array.Parallel.map (fun x -> (x,shouldIgnore x))
        |> Seq.filter (fun (x,ignore) -> not ignore)
        |> Seq.map (fun (x,ignore) -> x)
        |> List.ofSeq

    let itemsInProject =
        parseProject <| readAllText projectPath
        |> List.ofSeq

    let itemsOnDisk =
        getFilesFromDirectory projectDirectory projectDirectory
        |> Set.ofSeq

    let duplicateItems =
        findDuplicates itemsInProject

    let projectSet =
        itemsInProject
        |> List.map (fun x -> x.ToLower())
        |> Set.ofList

    let itemsNotInProjectTask =
        System.Threading.Tasks.Task.Factory.StartNew(fun () ->
            itemsOnDisk
            |> Seq.filter (fun x -> not (projectSet.Contains (x.ToLower())))
            |> filterIgnoredResultsInParallel
        )

    let itemsNotOnDiskTask =
        System.Threading.Tasks.Task.Factory.StartNew(fun () ->
            itemsInProject
            |> Seq.filter (fun x -> not (fileExists x))
            |> filterIgnoredResultsInParallel
        )

    new AnalysisResult(projectPath, duplicateItems, itemsNotInProjectTask.Result, itemsNotOnDiskTask.Result)
