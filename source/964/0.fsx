module Operators

// Division and multiplication operators which cast ints 
// to floats for themselves.  '.' on the side(s) which 
// need(s) to be cast.

/// Floating point division given int and double args.
let (./) x y = 
    (x |> double) / y

/// Floating point division given double and int args.
let (/.) x y = 
    x / (y |> double)

/// Floating point division given int and int args.
let (./.) x y = 
    (x |> double) / (y |> double)

/// Floating point multiplication given int and double args.
let (.*) x y = 
    (x |> double) * y

/// Floating point multiplication given double and int args.
let ( *. ) x y = 
    x * (y |> double)

/// Floating point multiplication given int and int args.
let (.*.) x y = 
    (x |> double) * (y |> double)

/// Examples:
let average = 
    let numbers = [0..100]
    (numbers |> List.sum) ./. (numbers |> List.length)

let average' = 
    let numbers = [0. .. 100.]
    (numbers |> List.sum) /. (numbers |> List.length)

let crazyTotal =
    [0. .. 100.]
    |> List.mapi (fun i x -> i .* x)
    |> List.sum
