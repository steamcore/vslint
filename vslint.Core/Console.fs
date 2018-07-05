
module vslint.Console

open System

/// <summary>
/// Prints formatted output to stdout, with color, adding a newline
/// </summary>
/// <param name="color">The color to print with</param>
/// <param name="fmt">The input formatter.</param>
let cprintfn color fmt =
    Printf.kprintf
        (fun str ->
            let foreground = Console.ForegroundColor
            Console.ForegroundColor <- color
            Console.WriteLine str
            Console.ForegroundColor <- foreground)
        fmt

/// <summary>
/// Prints an error message to stderr with red color
/// </summary>
/// <param name="fmt">The input formatter.</param>
let printError fmt =
    cprintfn ConsoleColor.Red fmt

/// <summary>
/// Prints a warning message to stderr with yellow color
/// </summary>
/// <param name="fmt">The input formatter.</param>
let printWarning fmt =
    cprintfn ConsoleColor.Yellow fmt
