
module vslint.Ignore

open System
open System.IO

open Cache
open IO
open GitSupport
open MercurialSupport

/// <summary>
/// Find all .vslintignore paths from a certain path up to a base path
/// </summary>
/// <param name="path">Directory to start searching in</param>
let rec findVslintIgnores (path : string) =
    let dotVslintIgnorePath = Path.Combine(path, ".vslintignore")
    let parentInfo = Directory.GetParent(path)
    if File.Exists(dotVslintIgnorePath) then [dotVslintIgnorePath]
    else if parentInfo = null then []
    else findVslintIgnores parentInfo.FullName

/// <summary>
/// Parse a .vslintignore file
/// The result is cached for future invocations
/// </summary>
/// <param name="lines">Path to .vslintignore file</param>
let parseVslintIgnore =
    let parseVslintIgnoreInternal (lines : list<string>) =
        lines
        |> List.filter (fun x -> not (x.StartsWith "#" || String.IsNullOrWhiteSpace x))
        |> List.map normalizeDirSeparator
    memoize parseVslintIgnoreInternal

/// <summary>
/// Retrieve ignore rules for a specific file, locates and utilizes .hgignore and .vslintignore files
/// </summary>
/// <param name="path">Path to project file</param>
let getIgnorePatterns path =

    // Default list of files to be ignored in analysis
    let defaultIgnorePatterns =
        seq [
            @"\.git\\";
            @"\.gitignore$";
            @"\.hg\\";
            @"\.hgignore$";
            @"\.svn\\";
            @"bin\\";
            @"obj\\";
            @"packages\\";
            @"\.sln$";
            @"\.(cs|fs|vb)proj$";
            @"\.targets$";
            @"\.suo$";
            @"\.user$";
            @"\.orig$";
        ]

    let projectDirectory = Path.GetDirectoryName path
    [
        defaultIgnorePatterns;
        findGitIgnores projectDirectory |> Seq.map readAllLines |> Seq.map parseGitIgnore |> Seq.concat;
        findHgIgnores projectDirectory |> Seq.map readAllLines |> Seq.map parseHgIgnore |> Seq.concat;
        findVslintIgnores projectDirectory |> Seq.map readAllLines |> Seq.map parseVslintIgnore |> Seq.concat;
    ]
    |> Seq.concat
    |> Seq.distinct
    |> List.ofSeq
