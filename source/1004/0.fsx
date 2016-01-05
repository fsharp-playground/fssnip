let powerset l =
    let s = ((l |> List.length) |> pown 2) - 1
    [0..s] |> 
    List.map (fun i -> l |> 
                       List.mapi (fun a b -> if (pown 2 a) &&& i <> 0 then Some(b) else None) |> 
                       List.choose id)


// powerset [1;2;3] = [[]; [1]; [2]; [1; 2]; [3]; [1; 3]; [2; 3]; [1; 2; 3]]