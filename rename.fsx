open System.IO
open System

let files = DirectoryInfo("./source").GetFiles("*", SearchOption.AllDirectories)
files |> Seq.iter(fun x ->
        Console.WriteLine x.FullName
        x.MoveTo(x.FullName + ".fsx"))
