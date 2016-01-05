open System.IO

let FilesPresentInDirectory basePath = 
    let countOfSubDirs = Directory.GetDirectories(basePath) |> Seq.length
    let countOfFiles = Directory.EnumerateFileSystemEntries(basePath) |> Seq.length
    countOfFiles > countOfSubDirs