
module vslint.MercurialSupportTests

open FsUnit.Xunit
open Xunit

open MercurialSupport

type ``parseHgIgnore tests``() =

    [<Fact>]
    member x.``given an empty list it should return an empty list `` () =
        parseHgIgnore [] |> should be Empty

    [<Fact>]
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

    [<Fact>]
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
