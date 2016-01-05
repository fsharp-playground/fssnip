///Simple gcd with ifs 
///Nice moment: all recs there transform to whiles.
let rec gcd a b =
    if b = 0L then a
    else gcd b (a % b)

(*[omit:(Simple gcd with match)]*)
let rec gcd' a b =
    match b with
    |0L -> a
    |_ -> gcd' b (a % b)
(*[/omit]*)

///Binary gcd with match
let rec gcdBin a b = 
    match a, b with
    |0L, _ |_, 0L -> a ||| b
    |_ when (a &&& 1L) ||| (b &&& 1L) = 0L ->
        gcdBin (a >>> 1) (b >>> 1) <<< 1
    |_ when a &&& 1L = 0L ->
        gcdBin (a >>> 1) b
    |_ when b &&& 1L = 0L ->
        gcdBin a (b >>> 1)
    |_ when a > b -> 
        gcdBin ((a - b) >>> 1) b
    |_ ->
        gcdBin ((b - a) >>> 1) a

(*[omit:(Binary gcd with ifs)]*)
let rec gcdBin' a b =
    if a = 0L || b = 0L then a ||| b
    else if (a &&& 1L) ||| (b &&& 1L) = 0L then 
        gcdBin' (a >>> 1) (b >>> 1) <<< 1
    else if a &&& 1L = 0L then gcdBin' (a >>> 1) b
    else if b &&& 1L = 0L then gcdBin' a (b >>> 1)
    else if a > b then gcdBin' ((a - b) >>> 1) b
    else gcdBin' ((b - a) >>> 1) a
(*[/omit]*)

(* The minimum number of unconcealed messages is 
(1 + gcd (e-1) (q-1)) * (1 + gcd (e-1) (p-1)) = 9
So the gcds there should be equal to 2 *)
let sum p q = 
    let phi = (p - 1L) * (q - 1L) in
        [3L..2L..phi - 1L] 
        //Realization using filter is maybe more beautiful, but less efficient
        |> List.sumBy (fun e -> 
            if gcd e phi = 1L &&
               gcd (e - 1L) (q - 1L) = 2L &&
               gcd (e - 1L) (p - 1L) = 2L 
            then e 
            else 0L)

printfn "%A" <| sum 1009L 3643L //399788195976L

(* The gcd test for 10000 random values says 'Be simple!':
simple gcd + if:    00.0058234
binary gcd + if:    00.0066124
binary gcd + match: 00.0067200
simple gcd + match: 00.0067880
*)