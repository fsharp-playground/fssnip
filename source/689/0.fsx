let powerset s =
    let rec loop n l =
        seq {
              match n, l with
              | 0, _  -> yield []
              | _, [] -> ()
              | n, x::xs -> yield! Seq.map (fun l -> x::l) (loop (n-1) xs)
                            yield! loop n xs
        }   
    let xs = s |> Set.toList     
    seq {
        for i = 0 to List.length xs do
            for x in loop i xs -> set x
    }
for st in Set.ofList [ 1..9] |> powerset  do
    printfn "%A" st