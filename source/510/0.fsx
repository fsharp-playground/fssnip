open System.Diagnostics

let f0 x = x + 1
let f1 x = x * 3

let f2 = f0 >> f1
let f3 x = x |> f0 |> f1

let sw = Stopwatch()
let N = 1000000

Seq.init 1 id |> Seq.map f2 |> Seq.length |> ignore

sw.Reset()
sw.Start()
Seq.init N id |> Seq.map f2 |> Seq.length |> ignore
sw.Stop()
printfn "f2 = %d" (sw.ElapsedMilliseconds)

sw.Reset()
sw.Start()
Seq.init N id |> Seq.map f3 |> Seq.length |> ignore
sw.Stop()
printfn "f3 = %d" (sw.ElapsedMilliseconds)   

System.Console.ReadKey() |> ignore
