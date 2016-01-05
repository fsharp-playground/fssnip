open System

let console = Seq.initInfinite (fun _ -> Console.ReadLine())
console |> Seq.iter (fun line ->
  printfn "You said %s" line
)