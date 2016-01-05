open System.IO

let FilesArePresentInDirectory dir = 
    (Directory.EnumerateFileSystemEntries(dir) |> Seq.length) > (Directory.GetDirectories(dir) |> Seq.length)