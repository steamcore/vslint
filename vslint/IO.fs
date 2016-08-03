
module vslint.IO

open System
open System.IO
open System.Text.RegularExpressions

open Cache

let directoriesToIgnore =
    set [
        ".git";
        ".hg";
        ".svn";
        "node_modules";
    ]

let private pathTooLong path =
    printfn "warning, path too long \"%s\"" path
    List.empty

let private unauthorizedAccess path =
    printfn "warning, access denied \"%s\"" path
    List.empty

/// <summary>
/// Returns a list of directory names in a specified path
/// The result is cached for future invocations
/// </summary>
/// <param name="path">Path to directory</param>
let enumerateDirectories =
    let enumerateDirectoriesInternal path =
        try
            Directory.EnumerateDirectories path
            |> Seq.cast<string>
            |> List.ofSeq
        with
            | :? System.IO.PathTooLongException -> pathTooLong path
            | :? System.UnauthorizedAccessException -> unauthorizedAccess path
    memoize enumerateDirectoriesInternal

/// <summary>
/// Returns a list of file names in a specified path
/// The result is cached for future invocations
/// </summary>
/// <param name="path">Path to directory</param>
let enumerateFiles =
    let enumerateFilesInternal path =
        try
            Directory.EnumerateFiles path
            |> Seq.cast<string>
            |> List.ofSeq
        with
            | :? System.IO.PathTooLongException -> pathTooLong path
            | :? System.UnauthorizedAccessException -> unauthorizedAccess path
    memoize enumerateFilesInternal

/// <summary>
/// Read all lines from a file
/// </summary>
/// <param name="path">Path to file</param>
let readAllLines path =
    File.ReadAllLines(path)
    |> List.ofArray<string>

/// <summary>
/// Read all text from a file
/// </summary>
/// <param name="path">Path to file</param>
let readAllText path =
    File.ReadAllText(path)

/// <summary>
/// Retrieves all files from a directory and it's subdirectories with paths relative to a starting point
/// </summary>
/// <param name="path">Root directory</param>
/// <param name="relativeTo">Make paths relative to this directory</param>
let rec getFilesFromDirectory (path : string) (relativeTo : string) =
    let filesFromSubdirectories =
        enumerateDirectories path
        |> Seq.filter (fun x -> not (directoriesToIgnore.Contains <| Path.GetFileName(x)))
        |> Array.ofSeq
        |> Array.Parallel.map (fun x -> getFilesFromDirectory x relativeTo)
        |> Seq.concat
    let files =
        enumerateFiles path
        |> Seq.map (fun x -> x.Replace(relativeTo, "").TrimStart('\\'))
    Seq.concat [filesFromSubdirectories; files]

/// <summary>
/// Recursively finds all project files
/// </summary>
/// <param name="path">Root directory</param>
let rec findProjectFiles (path : string) =
    let projectsFromSubdirectories =
        enumerateDirectories path
        |> Seq.filter (fun x -> not (directoriesToIgnore.Contains <| Path.GetFileName(x)))
        |> Array.ofSeq
        |> Array.Parallel.map findProjectFiles
        |> Seq.concat
    let projects =
        enumerateFiles path
        |> Seq.filter (fun x -> Regex.IsMatch(x, @"\.(fs|cs|vb)proj$"))
    Seq.concat [projectsFromSubdirectories; projects]

/// <summary>
/// Try to locate the source control root by 
/// </summary>
/// <param name="path">File or directory</param>
let rec tryFindSourceControlRoot (path : string) =
    if not (Directory.Exists path) then
        tryFindSourceControlRoot (Path.GetDirectoryName path)
    else
        let hasDotGit = enumerateDirectories path |> List.exists (fun x -> Path.GetFileName(x) = ".git")
        let hasDotHg = enumerateDirectories path |> List.exists (fun x -> Path.GetFileName(x) = ".hg")
        let parentInfo = Directory.GetParent path
        if hasDotGit || hasDotHg || parentInfo = null then path
        else tryFindSourceControlRoot parentInfo.FullName

/// <summary>
/// Converts a glob expression to a regular expression
/// Warning, this implementation will not handle complex cases
/// </summary>
/// <param name="value"></param>
let convertGlobToRegex (value : string) =
    value.Replace(".", @"\.").Replace("*", ".*")

/// <summary>
/// Replaces forward slashes with double backslashes for use in regular expression
/// </summary>
/// <param name="value"></param>
let normalizeDirSeparator (value : string) =
    value.Replace("/", @"\\")
