
module vslint.GitSupportTests

open NUnit.Framework
open FsUnit

open GitSupport

[<TestFixture>]
type ``parseGitIgnore tests``() =

    [<Test>]
    member x.``given an empty list it should return an empty list `` () =
        parseGitIgnore [] |> should equal []

    [<Test>]
    member x.``given simple globs it should return expected results`` () =
        let input =
            [
                @"packages/";
                @"*.dll";
                @"something.*";
            ]
        let expected =
            [
                @"packages\\";
                @".*\.dll";
                @"something\..*";
            ]
        parseGitIgnore input |> should equal expected
