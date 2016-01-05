#if INTERACTIVE
#r @"System.IO.Compression"
#r @"System.IO.Compression.FileSystem"
#endif

open System.IO
open System.Net
open System.IO.Compression

// Extract the contents of multiple zip archives in one pass.
//
// Eg. ExtractAll @"C:\Data\Corpora\GoogleNGrams\Compressed" @"C:\Data\Corpora\GoogleNGrams\Uncompressed"
//
let ExtractAll sourceDir targetDir =
    Directory.EnumerateFiles(sourceDir, "*.zip")
    |> Seq.iter (fun zipPath -> printfn "Uncompressing %s" (Path.GetFileName(zipPath))
                                let archive = ZipFile.Open(zipPath, ZipArchiveMode.Read)
                                archive.ExtractToDirectory(targetDir))