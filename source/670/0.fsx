//#r "System.Xml.dll" //for scripts or interactive
//#r "System.Xml.Linq.dll" //add reference if using F# as library
open System
open System.Xml.Linq
open System.IO
open System.Linq

///Fetch all *.csproj fiels
let getFiles = Directory.GetFiles(@"c:\projects\", "*.csproj", SearchOption.AllDirectories)
               |> Array.toSeq

///Linq-to-xml parsing
let parse (filename:string) =
    let xn ns s = XName.Get(s,ns)
    let xml = XDocument.Load filename
    let xns = xn (xml.Root.Attribute(XName.Get("xmlns")).Value)

    ///Tags of TreatWarningsAsErrors
    let elem = xml.Descendants(xns "TreatWarningsAsErrors")
    let values = elem |> Seq.map(fun x -> x.Value)
    (elem, values, xml, xns)

///Show file state: if active or deactive
let getProjectInfo (filename:string) = 
    let _, values, _, _ = parse filename

    match values.Any() with
    | true ->
        match values.Any(fun e -> e = "true") with
        | true -> "Active: " + filename //or just: ""
        | false -> "Disabled: " + filename //or just: ""
    | false ->
        "Not set: " + filename //or just: ""
 
///Manipulate file state
let modifyState (trueOrfalse:string) (filename:string) =     
    let elem, values, xml, xns = parse filename

    match values.Any() with
    | true ->
        //modify existing states
        let toModify = elem |> Seq.filter (fun el -> el.Value <> trueOrfalse)
                       
        match toModify.Any() with
        | true ->
            toModify |> Seq.iter (fun el -> el.Value <- trueOrfalse)
            //Maybe: version control checkout filename 
            //Or hijack, remove readonly: File.SetAttributes(filename, FileAttributes.Normal)
            xml.Save filename
            
            "Modified to " + trueOrfalse + ": " + filename
        | false ->
            "Was already " + trueOrfalse + ": " + filename
    | false ->
        //Add new element next to ProjectGuid
        let scc = xml.Element(xns "Project").Element(xns "PropertyGroup").Element(xns "ProjectGuid")
        let newXElement = new XElement(xns "TreatWarningsAsErrors", trueOrfalse)
        try //try: assume that ProjectGuid found
            scc.AddAfterSelf(newXElement)
            //Maybe: version control checkout filename 
            //Or hijack, remove readonly: File.SetAttributes(filename, FileAttributes.Normal)
            xml.Save filename
            "Added: " + filename
        with
        | _ -> "Failed: " + filename

//To show:
let showAllProjects = getFiles |> Seq.map (getProjectInfo) |> Seq.filter(fun f->f<>"") |> Seq.iter Console.WriteLine

//To modify:
//let setAllToFalse = getFiles |> Seq.map (modifyState "false") |> Seq.iter Console.WriteLine
//let setAllToTrue = getFiles |> Seq.map (modifyState "true") |> Seq.iter Console.WriteLine
