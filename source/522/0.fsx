//#r "System.Xml.dll" //for script or interactive
//#r "System.Xml.Linq.dll"
open System
open System.Xml.Linq
open System.IO

let xn ns s = XName.Get(s,ns)
 
let getFiles = Directory.GetFiles(@"C:\Users\", "*.csproj", SearchOption.AllDirectories) 
               |> Array.toSeq
 
let getProjectInfo (fname:string) = 
    let xml = XDocument.Load fname
    let xns = xn (xml.Root.Attribute(XName.Get("xmlns")).Value)

    let isSilverligthAssembly = 
       xml.Descendants(xns "TargetFrameworkIdentifier")
       |> Seq.filter (fun p -> p.Value = "Silverlight")
       |> Seq.isEmpty |> not

    let outputPaths = 
       xml.Descendants(xns "OutputPath")
       |> Seq.map(fun x -> x.Value)

    (fname, isSilverligthAssembly, outputPaths)

let showInfo projInfo =
    let name,sl,(outs:string seq) = projInfo
    match sl with
    | false -> Console.WriteLine("Assembly " + name + " outputs:") 
    | true -> Console.WriteLine("SL-assembly " + name + " outputs:")    
    outs |> Seq.iter(Console.WriteLine)

let test = getFiles 
           |> Seq.map (getProjectInfo)
           |> Seq.iter(showInfo)
