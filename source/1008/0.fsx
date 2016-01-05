open Microsoft.FSharp.Reflection

let split (unions : 'T seq) =
    let reader = FSharpValue.PreComputeUnionTagReader typeof<'T>
    unions  |> Seq.groupBy reader 
            |> Seq.map (fun (x,y) -> x, Seq.toList y)
            |> Map.ofSeq

// example

type Foo = A | B | C | D | E

let map = [ A ; B ; C ; D ; A ; A ; B ; C ; D ; B ; B ; A ] |> split

if map.ContainsKey 0 && not <| map.ContainsKey 4 then 
    printfn "the sequence contains an A but not an E."