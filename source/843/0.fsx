#r "System.Xml.Linq.dll"

open System.IO
open System.Xml
open System.Xml.Linq

let [<Literal>] NS = "http://schemas.microsoft.com/developer/msbuild/2003"
let vs_MinVer = XName.Get("MinimumVisualStudioVersion",NS)
let propGroup = XName.Get("Propertygroup",NS)

let update file action =
  printfn "Loading '%s'..." file
  try
    let xd = XDocument.Load(file)
    printfn "Updating file..."
    action xd.Root
    printfn "Saving changes..."
    xd.Save(file)
    printfn "Success!"
  with
    | x -> printfn "Error: %A" x

let updateAll path pattern action =
  Directory.EnumerateFiles(path,pattern,SearchOption.AllDirectories)
  |> Seq.iter (fun file -> update file action)

updateAll @"c:\working\fs-zmq"
          "*.fsproj" 
          (fun xe -> xe.Descendants(vs_MinVer)
                       .Ancestors(propGroup)
                       .Remove())
