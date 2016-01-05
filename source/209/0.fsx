let curry f a b = f (a,b)
let uncurry f (a,b) = f a b

let ArrayMultiply a b = Array.zip a b |> Array.map (uncurry (*))