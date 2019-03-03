
module vslint.GitSupportTests

open FsUnit.Xunit
open Xunit

open GitSupport

type ``parseGitIgnore tests``() =

    [<Fact>]
    member x.``given an empty list it should return an empty list `` () =
        parseGitIgnore [] |> should be Empty

    [<Fact>]
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
