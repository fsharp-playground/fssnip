let inline ZipMap f a b = Seq.zip a b |> Seq.map (fun (x,y) -> f x y)
let inline mult a b = ZipMap (*) a b
mult [1;2;3] [4;5;6]
