open System
let ran = Random()

/// Flip a coin with probability p for true
let dice p = ran.NextDouble() <= p

/// Random walk from zero stepping up and down according to dice p
let walk p =
    Seq.unfold (fun z -> let z = if dice p then z+1 else z-1
                         Some (z,z)) 0

/// First n steps
let walkFor n = walk 0.5 |> Seq.take n
