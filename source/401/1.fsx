module Seq =
    
    let group (p:'a -> 'a -> bool) (s:'a seq) = 
        if s |> Seq.isEmpty then seq []
        else
            let h = s |> Seq.head
            let state = ( (h,seq [h]) , Seq.empty)
            let ((a,b),c) = 
                s |> Seq.skip 1 
                |> Seq.fold (fun ( (a,b),c) i -> 
                                if p a i then ( (a,seq { yield! b; yield i}) ,c)
                                else ( (i, seq {yield i}), seq {yield! c; yield b}) ) 
                                state 
            seq { yield! c; yield b}

[1;2;3;4;4;4;4] |> Seq.group (=) |> printfn "%A"