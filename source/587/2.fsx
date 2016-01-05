open System

/// Euclid's algorithm for hcf
let rec highestCommonFactor (x : int) (y : int) =
    let larger, smaller = Math.Max(x, y), Math.Min(x, y)
    let q, r = larger / smaller, larger % smaller
    if r = 0 then
        smaller
    else
        highestCommonFactor y r

/// Find the lowest terms of a fraction - eg. 2/4 --> 1/2
let lowestTerms n d = 
    if n <= 0 || d <= 0 then
        1, 1
    else
        let hcf = highestCommonFactor n d 
        n / hcf, d / hcf

/// Convert a floating point number to the nearest mixed number using the 
/// specified numerator and denominator - eg. 1,3,1.666 --> 1 2/3
let toMixedNumber numerator denominator x =
    if denominator <= 0 then
        raise (System.ArgumentException("Denominator must be greater than 0"))
    let a = x / (numerator|>float) * (denominator|>float)
    let b = a |> round |> int
    let c = b / denominator
    let d = b % denominator

    let d', denominator' = lowestTerms d denominator

    match c, d with
    | 0, 0 -> sprintf "0"
    | 0, d -> sprintf "%i/%i" d' denominator'
    | c, 0 -> sprintf "%i" c
    | c, d -> sprintf "%i %i/%i" c d' denominator'

//  ["0"; "1/3"; "2/3"; "1"; "1 1/3"; "1 2/3"; "2"; "2 1/3"; "2 2/3"; "3";
//   "3 1/3"; "3 2/3"; "4"; "4 1/3"; "4 2/3"; "5"; "5 1/3"; "5 2/3"; "6";
//   "6 1/3"; "6 2/3"; "7"; "7 1/3"; "7 2/3"; "8"; "8 1/3"; "8 2/3"; "9";
[0. .. (1./3.) .. 32.] |> List.map (fun x -> toMixedNumber 1 3 x)

//  ["0"; "1/6"; "1/3"; "1/2"; "2/3"; "5/6"; "1"; "1 1/6"; "1 1/3"; "1 1/2";
//   "1 2/3"; "1 5/6"; "2"; "2 1/6"; "2 1/3"; "2 1/2"; "2 2/3"; "2 5/6"; "3";
//   "3 1/6"; "3 1/3"; "3 1/2"; "3 2/3"; "3 5/6"; "4"; "4 1/6"; "4 1/3"; "4 1/2";
[0. .. (1./6.) .. 32.] |> List.map (fun x -> toMixedNumber 1 6 x)

//  ["0"; "1/10"; "1/5"; "3/10"; "2/5"; "1/2"; "3/5"; "7/10"; "4/5"; "9/10"; "1";
//   "1 1/10"; "1 1/5"; "1 3/10"; "1 2/5"; "1 1/2"; "1 3/5"; "1 7/10"; "1 4/5";
//   "1 9/10"; "2"; "2 1/10"; "2 1/5"; "2 3/10"; "2 2/5"; "2 1/2"; "2 3/5";
[0. .. (1./10.) .. 32.] |> List.map (fun x -> toMixedNumber 1 10 x)
