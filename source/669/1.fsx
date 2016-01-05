open System

module Seq =

    // As Seq.filter but provide i as a parameter to the filter function.
    let filteri f s =
        s
        |> Seq.mapi (fun i elem -> i, elem)
        |> Seq.filter (fun (i, elem) -> f i elem)
        |> Seq.map (fun (_, elem) -> elem)

module Sample =

    let sample size seq =
        // Get sample length:
        let len = seq |> Seq.length
        // Handle request for sample from empty sequence:
        if size = 0 then
            Seq.empty
        else
            // Approximate an interval between samples:
            let interval = Math.Max(len / size, 1)
            // Get the sample, truncating in case rounding meant too many elements:
            seq
            |> Seq.filteri (fun i _ -> i % interval = 0)
            |> Seq.truncate size

open Sample
            
let eleven = [1..11] 

// seq [1]
let test1 = eleven |> sample 1

// seq [1; 7]
let test2 = eleven |> sample 2

// seq [1; 5; 9]
let test3 = eleven |> sample 3

// seq [1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11]  
let test11 = eleven |> sample 11

// seq [1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11]  
let test12 = eleven |> sample 12