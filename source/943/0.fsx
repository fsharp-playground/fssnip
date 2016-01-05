let until init upper =
    Seq.initInfinite (fun x -> x + init)
    |> Seq.takeWhile (fun x -> x < upper)
    |> Seq.toList

// usage
until 1 9
|> printfn "%A"
(* #=> val it : int list = [1; 2; 3; 4; 5; 6; 7; 8] *)