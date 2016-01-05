open System
open System.IO

// Wait until a file has completed copying, writing etc - if it exists at all:
let rec fileQuiet path =
    try
        if File.Exists(path) then
            use f = File.OpenRead(path)
            true
        else
            true // Change this if you want to know if the file didn't exist
    with
    | _ -> System.Threading.Thread.Sleep(1000)
           fileQuiet path