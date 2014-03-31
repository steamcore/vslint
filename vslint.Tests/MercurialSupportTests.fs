
module vslint.MercurialSupportTests

open NUnit.Framework
open FsUnit

open MercurialSupport

[<TestFixture>]
type ``parseHgIgnore tests``() =

    [<Test>]
    member x.``given an empty list it should return an empty list `` () =
        parseHgIgnore [] |> should equal []

    [<Test>]
    member x.``given simple regexes it should return expected results`` () =
        let input =
            [
                @"test\\";
                @"packages/";
                @"\.dll$";
            ]
        let expected =
            [
                @"test\\";
                @"packages\\";
                @"\.dll$";
            ]
        parseHgIgnore input |> should equal expected

    [<Test>]
    member x.``given complex input it should return expected results`` () =
        let input =
            [
                @"# comment";
                @"syntax: glob";
                @"test/*.user";
                @"";
                @"# another comment";
                @"syntax: regexp";
                @"packages/";
                @"\.dll$";
            ]
        let expected =
            [
                @"test\\.*\.user";
                @"packages\\";
                @"\.dll$";
            ]
        parseHgIgnore input |> should equal expected
