open System;

let echo line = printfn "You said %s" line

let infiniteLines = Seq.initInfinite (fun _ -> Console.ReadLine())

Seq.iter echo infiniteLines

