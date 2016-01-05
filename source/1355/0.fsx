// reference FSharp.Core v4.3.0.0
// reference Microsoft.VisualBasic v10.0.0.0

open System
open System.Reflection
open Microsoft.VisualBasic

open System.IO

let asm_id name version typ pkt =
  sprintf """<assemblyIdentity name="%s" version="%s" type="%s" publicKeyToken="%s" />"""
    name version typ pkt

let clr_class name clsid progid =
  sprintf
    """<clrClass name="%s" clsid="{%s}" progid="%s" threadingModel="Both" />"""
    name clsid progid

let to_hex arr = BitConverter.ToString(arr).Replace("-", "")

let type_name (typ : Type) =
  sprintf "%s.%s" (typ.Namespace) (typ.Name)

let in_directory dir f =
  let curr = Environment.CurrentDirectory
  Environment.CurrentDirectory <- dir
  try
    f ()
  finally
    Environment.CurrentDirectory <- curr

[<EntryPoint>]
let main argv =
  let file = argv.[0]
  let path = Path.GetFullPath file

  in_directory (Path.GetDirectoryName path) <| fun () ->
    let asm = Assembly.LoadFrom path
    let asm_name = asm.GetName()

    let clr_classes () =
      asm.GetTypes()
      |> Array.toList
      |> List.map (fun t -> t, t.GetCustomAttributes(typeof<ComClassAttribute>, true))
      |> List.filter (fun (t, attrs) -> attrs.Length > 0)
      |> List.map (fun (t, attrs) -> t, (Array.get attrs 0 :?> ComClassAttribute))
      |> List.map (fun (t, attr) -> clr_class (type_name t) attr.ClassID (type_name t))

    try
      printfn """<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">"""
      printfn "  %s" <| asm_id
                            (Path.GetFileNameWithoutExtension(file))
                            (asm_name.Version.ToString())
                            (asm_name.ProcessorArchitecture.ToString())
                            (asm_name.GetPublicKeyToken() |> to_hex)
      printfn """  <description>Copyright: 2000-2014 (c) Intelliplan AB. All rights reserved.</description>"""
      clr_classes () |> List.iter (printfn "  %s")
      printfn "</assembly>"
      0
    with
    | :? ReflectionTypeLoadException as e ->
      eprintfn "%s" "Error while loading types: "
      e.LoaderExceptions |> Array.iter (eprintfn "%O")
      1
