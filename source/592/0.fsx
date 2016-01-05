/// Exclusive 'between' operator:
let (><) x (min, max) =
    (x > min) && (x < max)

/// Inclusive 'between' operator:
let (>=<) x (min, max) =
    (x >= min) && (x <= max)

// Examples
let four = [0..9] |> List.filter (fun x -> x >< (3, 5))
// val four : int list = [4]

let threeToFive = [0..9] |> List.filter (fun x -> x >=< (3, 5))
// val threeToFive : int list = [3; 4; 5]