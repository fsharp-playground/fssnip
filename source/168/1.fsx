open System;

let getLine  = fun _ -> Console.ReadLine()
let lines = Seq.initInfinite getLine

let echo line = printfn "You said %s" line

Seq.iter echo lines
