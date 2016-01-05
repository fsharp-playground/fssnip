#if INTERACTIVE
#r @"C:\SK\v9.2\packages\DotNetZip.1.9.1.8\lib\net20\Ionic.Zip.dll"
#else
module FsSnip.NuGet
#endif

open System
open System.IO
open System.Net
open Ionic.Zip

let getPackageUrl = sprintf "http://packages.nuget.org/api/v1/package/%s/"

let getPackage name = async {
    let url = getPackageUrl name
    let req = WebRequest.Create(url)
    use! resp = req.AsyncGetResponse()
    use incoming = resp.GetResponseStream()
    
    // The response stream doesn't support Seek, and ZipFile needs it,
    // so read into a MemoryStream
    use ms = new MemoryStream()
    let buffer = Array.zeroCreate<byte> 0x1000
    let more = ref true
    while !more do
        let! read = incoming.AsyncRead(buffer, 0, buffer.Length)
        if read = 0 then
            more := false
        else
            ms.Write(buffer, 0, read)

    ms.Seek(0L, SeekOrigin.Begin) |> ignore
    use zip = ZipFile.Read(ms)
    let tempName = Path.GetTempFileName()
    File.Delete tempName // GetTempFileName creates a file, we want a folder
    let dir = Directory.CreateDirectory tempName
    zip.ExtractAll tempName
    return dir
}

let ensureDelete (dir:DirectoryInfo) =
    { new IDisposable with
        member x.Dispose() =
            dir.Delete(true) }

let findAssemblies (dir:DirectoryInfo) =
    let rec filterAssemblies (dir:DirectoryInfo) = seq {
        for item in dir.EnumerateFileSystemInfos() do
            match item with
            | :? DirectoryInfo as d ->
                match d.Name with
                | "lib" | "net20" | "net35" | "net40" | "net40-full" | "net40-client" | "net45" ->
                    yield! filterAssemblies d
                | _ -> ()
            | :? FileInfo as f ->
                if f.Extension = ".dll" then
                    yield f
            | _ -> ()
    }

    let getPlatform (f:FileInfo) =
        f.DirectoryName.[f.DirectoryName.LastIndexOf('\\') + 1 ..]

    let allAssemblies =
        filterAssemblies dir
        |> Seq.cache

    let platforms =
        allAssemblies
        |> Seq.map getPlatform
        |> Set.ofSeq

    let newestPlatforms =
        platforms
        |> Set.filter 
            (function
            | "lib" | "net45" -> true
            | "net20" -> 
                [ "net35"; "net40"; "net40-full"; "net40-client"; "net45" ] 
                |> Set.ofList 
                |> Set.intersect platforms 
                |> Set.isEmpty
            | "net35" ->
                [ "net40"; "net40-full"; "net40-client"; "net45" ] 
                |> Set.ofList 
                |> Set.intersect platforms 
                |> Set.isEmpty
            | "net40-client" ->
                [ "net40"; "net40-full"; "net45" ] 
                |> Set.ofList 
                |> Set.intersect platforms 
                |> Set.isEmpty
            | "net40" ->
                [ "net40-full"; "net45" ] 
                |> Set.ofList 
                |> Set.intersect platforms 
                |> Set.isEmpty
            | "net40-full" ->
                platforms.Contains "net45" |> not
            | _ -> false)

    allAssemblies
    |> Seq.filter (getPlatform >> newestPlatforms.Contains)

let printFiles name =
    printfn "Downloading %s..." name
    let packageDir =
        getPackage name
        |> Async.RunSynchronously

    use x = ensureDelete packageDir

    findAssemblies packageDir
    |> Seq.iter (fun f -> printfn "%s" f.FullName)

(*
printFiles "Castle.Core"
printFiles "DotNetZip"
printFiles "RavenDB.Client"
printFiles "xunit"
printFiles "AutoMapper"
printFiles "EntityFramework"
printFiles "Newtonsoft.Json"
*)