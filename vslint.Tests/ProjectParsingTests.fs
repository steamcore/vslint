
module vslint.ProjectParsingTests

open NUnit.Framework
open FsUnit

open ProjectParsing

[<TestFixture>]
type ``parseProject tests``() =

    [<Test>]
    member x.``given a classic project, should return type ClassicProject`` () =
        let content = "<Project ToolsVersion=\"12.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>"
        let project = parseProject content
        match project with
        | Project.Classic c -> ()
        | _ -> Assert.Fail("Wrong type")

    [<Test>]
    member x.``given a modern project, should return type ModernProject`` () =
        let content = "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>"
        let project = parseProject content
        match project with
        | Project.Modern m -> ()
        | _ -> Assert.Fail("Wrong type")
