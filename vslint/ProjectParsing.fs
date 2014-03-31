
module ProjectParsing

open System
open System.Web
open System.Xml
open System.Xml.Linq
open System.Xml.XPath

/// <summary>
/// Parse contents of a Visual Studio project file and returns a list of included files
/// </summary>
/// <param name="content">Contents of project file</param>
let parseProject (content : string) =
    let namespaces =
        let projectns = "http://schemas.microsoft.com/developer/msbuild/2003"
        let nsmgr = new XmlNamespaceManager(new NameTable())
        nsmgr.AddNamespace("ns", projectns)
        nsmgr
    let excludedTypes =
        [
            "AppDesigner";
            "BootstrapperPackage";
            "CodeAnalysisDependentAssemblyPaths";
            "Reference";
            "PublishFile";
            "Service";
        ]
    let xpath = "//*[@Include]" + String.Join("", (excludedTypes |> Seq.map (fun x -> "[not(self::ns:" + x + ")]")))
    let doc = XDocument.Parse content
    doc.XPathSelectElements(xpath, namespaces)
        |> Seq.cast<XElement>
        |> Seq.map (fun x -> (x.Attribute <| XName.Get "Include"))
        |> Seq.map (fun x -> x.Value)
        |> Seq.map HttpUtility.UrlDecode
        |> Seq.filter (fun x -> not (x.EndsWith @"\"))
