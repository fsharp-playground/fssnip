open System.IO

let AreFilesPresentInDirectory dir = 
    (Directory.EnumerateFileSystemEntries(dir) |> Seq.length) > (Directory.GetDirectories(dir) |> Seq.length)