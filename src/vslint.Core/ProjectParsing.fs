
module ProjectParsing

open System
open System.Xml
open System.Xml.Linq
open System.Xml.XPath

let excludedTypes =
    [
        "AppDesigner";
        "BootstrapperPackage";
        "CodeAnalysisDependentAssemblyPaths";
        "DotNetCliToolReference";
        "Reference";
        "PackageReference";
        "ProjectCapability";
        "ProjectReference";
        "PublishFile";
        "Service";
    ]

let readIncludes (doc : XDocument, xpath : string, namespaces : XmlNamespaceManager) =
    doc.XPathSelectElements(xpath, namespaces)
    |> Seq.cast<XElement>
    |> Seq.map (fun x -> (x.Attribute <| XName.Get "Include"))
    |> Seq.map (fun x -> x.Value)
    |> Seq.map Uri.UnescapeDataString
    |> Seq.filter (fun x -> not (x.EndsWith @"\" || x.Contains "*"))
    |> List.ofSeq

/// <summary>
/// Represents a classic MSBuild project where all files needs to be explicitly included
/// </summary>
type ClassicProject (doc : XDocument) =
    let projectItems =
        let namespaces =
            let projectns = "http://schemas.microsoft.com/developer/msbuild/2003"
            let nsmgr = new XmlNamespaceManager(new NameTable())
            nsmgr.AddNamespace("ns", projectns)
            nsmgr
        let xpath = "//*[@Include]" + String.Join("", (excludedTypes |> Seq.map (fun x -> "[not(self::ns:" + x + ")]")))
        readIncludes(doc, xpath, namespaces)

    let itemSet =
        projectItems
        |> List.map (fun x -> x.ToLower())
        |> Set.ofList

    member this.items = projectItems

    member this.isIncluded (path : string) =
        let normalizedPath = path.ToLower()
        itemSet.Contains normalizedPath

/// <summary>
/// Represents modern MSBuild projects where most files (if not all) are automatically included
/// </summary>
type ModernProject (doc : XDocument) =
    let projectItems =
        let namespaces = new XmlNamespaceManager(new NameTable())
        let xpath = "//*[@Include]" + String.Join("", (excludedTypes |> Seq.map (fun x -> "[not(self::" + x + ")]")))
        readIncludes(doc, xpath, namespaces)

    member this.items = projectItems

    // For now let's just say everything is included
    member this.isIncluded (path : string) =
        true

type Project =
    | Classic of ClassicProject
    | Modern of ModernProject

    member self.items =
        match self with
        | Classic c -> c.items
        | Modern m -> m.items

    member self.isIncluded =
        match self with
        | Classic c -> c.isIncluded
        | Modern m -> m.isIncluded

/// <summary>
/// Parse contents of a Visual Studio project file and returns a list of included files
/// </summary>
/// <param name="content">Contents of project file</param>
let parseProject (content : string) : Project =
    let doc = XDocument.Parse content
    let sdk = doc.Root.Attribute(XName.Get("Sdk"))
    if sdk <> null && sdk.Value.StartsWith "Microsoft.NET"
    then Modern (ModernProject doc)
    else Classic (ClassicProject doc)
