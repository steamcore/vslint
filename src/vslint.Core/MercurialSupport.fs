
module vslint.MercurialSupport

open System
open System.IO
open System.Text.RegularExpressions

open Cache
open IO

/// <summary>
/// Find all .hgignore paths from a certain path up to a base path
/// </summary>
/// <param name="path">Directory to start searching in</param>
let rec findHgIgnores (path : string) =
    let dotHgPath = Path.Combine(path, ".hg")
    let dotHgIgnorePath = Path.Combine(path, ".hgignore")
    let parentInfo = Directory.GetParent(path)
    if Directory.Exists(dotHgPath) && File.Exists(dotHgIgnorePath) then [dotHgIgnorePath]
    elif parentInfo = null then []
    else findHgIgnores parentInfo.FullName

/// <summary>
/// Parse a .hgignore file and convert glob expressions to regular expressions
/// The result is cached for future invocations
/// </summary>
/// <param name="lines">Contents of .hgignore file to parse</param>
let parseHgIgnore =

    let parseHgIgnoreInternal (lines : list<string>) =

        // Recursively parses a list of lines from a .hgignore
        let rec parse (mode : string) (patterns : list<string>) =
            if patterns.IsEmpty then
                List.empty<string>
            elif patterns.Head.StartsWith("syntax:") then
                let syntax = Regex.Match(patterns.Head, @"syntax:\s*(\w+)").Groups.[1].Value
                parse syntax patterns.Tail
            else
                let normalizedHead = normalizeDirSeparator patterns.Head
                match mode with
                | "regexp" -> normalizedHead :: parse mode patterns.Tail
                | "glob" -> (convertGlobToRegex normalizedHead) :: parse mode patterns.Tail
                | _ -> List.empty<string>

        // Remove comments and empty lines
        let patterns = lines |> List.filter (fun x -> not (x.StartsWith "#" || String.IsNullOrWhiteSpace x))

        parse "regexp" patterns

    memoize parseHgIgnoreInternal
