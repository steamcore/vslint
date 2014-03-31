
module vslint.GitSupport

open System
open System.IO

open Cache
open IO

/// <summary>
/// Find all .gitignore paths from a certain path up to a base path
/// </summary>
/// <param name="path">Directory to start searching in</param>
let rec findGitIgnores (path : string) =
    let dotGitPath = Path.Combine(path, ".git")
    let dotGitIgnorePath = Path.Combine(path, ".gitignore")
    let parentInfo = Directory.GetParent(path)
    if Directory.Exists(dotGitPath) && File.Exists(dotGitIgnorePath) then [dotGitIgnorePath]
    elif parentInfo = null then []
    else findGitIgnores parentInfo.FullName

/// <summary>
/// Parse a .gitignore file
/// The result is cached for future invocations
/// </summary>
/// <param name="lines">Path to .gitignore file</param>
let parseGitIgnore =
    let parseGitIgnoreInternal (lines : list<string>) =
        lines
        |> List.filter (fun x -> not (x.StartsWith "#" || String.IsNullOrWhiteSpace x))
        |> List.map normalizeDirSeparator
        |> List.map convertGlobToRegex
    memoize parseGitIgnoreInternal
