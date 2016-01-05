let inline (^<|) f a = f a

module AssociativityComparison =
    let forward_pipe = 
        {1..10} |> Seq.map (fun x -> x + 3) |> Seq.toList
    
    let normal_backward_pipe = 
        Seq.toList <| (Seq.map (fun x -> x + 3) <| {1..10})
    
    let high_prec_right_assoc_backward_pipe = 
        Seq.toList ^<| Seq.map (fun x -> x + 3) ^<| {1..10}

module PrecedenceComparison =
    let normal_backward_pipe  = 
        {1..10} |> (Seq.map <| (fun x -> x + 3))
   
    let high_prec_right_assoc_backward_pipe = 
        {1..10} |> Seq.map ^<| fun x -> x + 3
