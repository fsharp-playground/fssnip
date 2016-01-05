// annoyed with resources embedded in resources in assemblies?
// extract all images from all assemblies in a folder
// see call commented at the end
#r "System.Drawing"
open System.Collections
open System.Drawing
open System.Linq
open System.IO
open System.Resources
open System.Reflection
open System.Drawing.Imaging
 
let getFilenameExtension (format: ImageFormat) =
  let extensions = ImageCodecInfo.GetImageEncoders().FirstOrDefault(fun e -> e.FormatID = format.Guid).FilenameExtension
  (extensions.Split(';').[0]).ToLower().Replace("*","")
 
let extractResourcesImages rootFolder (a:Assembly)  =
  let directory = Path.Combine(rootFolder, "extractedimages", a.GetName().Name)
  ignore <| Directory.CreateDirectory(directory)
 
  let resourceNames = a.GetManifestResourceNames()
  for n in resourceNames do
    printfn "resource name: %s" n
    match n with
    | x when x.EndsWith("resources") -> 
      use stream = a.GetManifestResourceStream(n)
      use rm = new ResourceReader(stream)
      for entry in rm.Cast<DictionaryEntry>() do
        match entry.Value with
        | :? Bitmap -> 
          let bitmap = (entry.Value :?> Bitmap)
          
          let filename = Path.Combine(directory, n, entry.Key.ToString()) + (getFilenameExtension (bitmap.RawFormat))
          let file = new FileInfo(filename)
          file.Directory.Create()
          let image = new Bitmap(bitmap)
          try
            image.Save(filename, bitmap.RawFormat)
            printfn "saved %s" filename
          with
          | e -> printfn "failed to save resource %s %s" filename (e.ToString())
          
        | _ -> ()
        printfn "%O %O" entry.Key (entry.Value.GetType().Name)
    | _ -> ()
 
let loadAllAssemblies folder =
  let folder = new DirectoryInfo(folder)
  let files = seq {
    yield! folder.GetFiles("*.exe")
    yield! folder.GetFiles("*.dll")
  }
  
  let tryLoadAssembly f =
      try
        Some <| Assembly.LoadFile(f)
      with
      | e -> None
 
  seq {
    for f in files do
      let a = tryLoadAssembly f.FullName
      if a.IsSome then
        yield a.Value
  }
  
(*
loadAllAssemblies (Path.Combine(__SOURCE_DIRECTORY__ , "../../../bin/debug/"))
|> Seq.iter (extractResourcesImages __SOURCE_DIRECTORY__)
*)