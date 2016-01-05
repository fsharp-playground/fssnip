let rec selections l = 
        match l with 
        | [] -> []
        | (x::xs) -> (x,xs) :: [for (y,ys) in (selections xs) -> (y, x::ys)]

selections [1..2000] |> List.length
//above takes 756ms on my macbook

//the same in haskell takes approx 360ms.
//any technique to optimize F#??