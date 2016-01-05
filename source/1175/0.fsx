open System
let randGen = new Random()

let producer = 
    Seq.initInfinite (fun _ -> randGen.Next())

let consumer =
    Seq.initInfinite (fun _ -> (printfn "got %i"))
    
let combine producer consumer = 
    Seq.map2 (fun p c -> c p) producer consumer
    
combine producer consumer |> Seq.take 100 |> Seq.toArray |> ignore