module Seq =
    let infiniteOf repeatedList = 
        Seq.initInfinite (fun _ -> repeatedList) 
        |> Seq.concat

// Tests
let intList = [1; 2; 3; 4]
let charList = ['a'; 'b'; 'c'; 'd']
let objList = [(new System.Object()); (new System.Object()); (new System.Object()); (new System.Object())]
do
    Seq.infiniteOf intList |> Seq.take 20 |> Seq.iter (fun item -> printfn "%A" item)
    Seq.infiniteOf charList |> Seq.take 20 |> Seq.iter (fun item -> printfn "%A" item)
    Seq.infiniteOf objList |> Seq.take 20 |> Seq.iter (fun item -> printfn "%A" item)