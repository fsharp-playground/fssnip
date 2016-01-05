let curry f a b = f (a,b)
let uncurry f (a,b) = f a b

let inline ZipMap f a b = Seq.zip a b |> Seq.map (uncurry f)
let inline seqMult a b = ZipMap (*) a b

seqMult [1.0; 2.0; 3.5] [2.0; 3.1; 4.0]