module Program

open System.IO

let EnumerateDirectoryFilesInfo root = 
    let rec traverse (d: DirectoryInfo) = 
        seq {   for f in d.GetFiles() do
                    yield f
                for dd in d.GetDirectories() do
                    yield! traverse dd              }
    traverse (DirectoryInfo( root ))
    
EnumerateDirectoryFilesInfo @"C:\Temp\Input"
|> Seq.iter( fun f -> printfn "%s" f.FullName )