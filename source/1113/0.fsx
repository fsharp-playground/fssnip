let sqr x = x * x


let sumOfSquaresI nums = 
    let mutable acc = 0.0
    for x in nums do
        acc <- acc + sqr x
    acc

let rec sumOfSquaresF nums =
    match nums with
    | hd::tl -> sqr hd + sumOfSquaresF tl
    | []      -> 0.0


let sumOfSquares nums = 
    nums
    |> Seq.map sqr
    |> Seq.sum