open System
open System.IO

let sizeOfFolder folder =

    // Get all files under the path
    let filesInFolder : string []  = 
        Directory.GetFiles(
            folder, "*.*", 
            SearchOption.AllDirectories)

    // Map those files to their corresponding FileInfo object
    let fileInfos : FileInfo [] = 
        Array.map 
            (fun file -> new FileInfo(file)) 
            filesInFolder

    // Map those fileInfo objects to the file's size
    let fileSizes : int64 [] = 
        Array.map 
            (fun (info : FileInfo) -> info.Length) 
            fileInfos

    // Total the file sizes
    let totalSize = Array.sum fileSizes

    // Return the total size of the files
    totalSize
