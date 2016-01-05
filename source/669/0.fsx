open System

let sample size seq =
    // Get sample length:
    let len = seq |> Seq.length
    // Handle request for empty sample:
    if size = 0 then
        Seq.empty
    else
        // Handle request for sample bigger than input seq:
        if size > len then
            seq
        else
            // Approximate an interval between samples:
            let interval = int(Math.Round(float(len) / float(size)))
            // Get most of the sample - may get one too few or one to many due to rounding:
            let basic = 
                seq
                |> Seq.mapi (fun i elem -> i, elem)
                |> Seq.filter (fun pair -> let i = fst(pair)
                                           i % interval = 0)
                |> Seq.map (fun pair -> snd(pair))
            // Either shorten or add to the sequence to get the exact required sample:
            let adjusted =
                if (basic |> Seq.length) >= size then
                    basic |> Seq.truncate size
                else
                    let last = seq |> Seq.nth (len-1)
                    Seq.append basic [last]
            adjusted

let eleven = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11]  

// seq [1]
let test1 = eleven |> sample 1

// seq [1; 7]
let test2 = eleven |> sample 2

// seq [1; 5; 9]
let test3 = eleven |> sample 3
