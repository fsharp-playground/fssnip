module DirectoryExtensions

open System
open System.IO

let public SafeEnumerateFiles(path : string, searchPattern: string) =
    let safeEnumerate f path =
        try
            f(path)
        with
            | :? System.UnauthorizedAccessException -> Seq.empty

    let enumerateDirs = 
        safeEnumerate Directory.EnumerateDirectories

    let enumerateFiles =
        safeEnumerate (fun path -> Directory.EnumerateFiles(path, searchPattern))

    let rec enumerate baseDir =
        seq {
            yield! enumerateFiles baseDir
        
            for dir in enumerateDirs baseDir do
                yield! enumerate dir
        }

    enumerate path

// test
SafeEnumerateFiles(@".\clojure-1.4.0\", "*") |> Seq.iter (printfn "%s")