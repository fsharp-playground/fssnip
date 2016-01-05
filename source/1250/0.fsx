open System.IO

let path = @"C:\Users\<User Name>\Documents\Visual Studio 2012\Projects\"

let purge name =
    let dirs = Directory.EnumerateDirectories(path, name, SearchOption.AllDirectories)
    for dir in dirs do
        printfn "Found %s" dir
        let files = Directory.EnumerateFiles(dir,"*.*",SearchOption.AllDirectories) |> Seq.toArray   
        for file in files do
            printfn "Deleting %s" file
            File.Delete(file)

purge "bin"
purge "obj"