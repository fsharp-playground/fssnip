open System

// arr1 is always equal in size or smaller than arr2
let bestRot (f: 'a -> 'a -> float) (arr1: 'a []) (arr2: 'a []) =
    // Pre-calculate similarities
    let scores = 
        Array2D.init arr1.Length arr2.Length 
                    (fun i j -> f arr1.[i] arr2.[j])
    // inner function for recursively finding paths
    // col = previous column, prow = previous row
    // df = degrees of freedom, path = path accumulator
    let rec inner col prow df path =
        seq {
            // when we're out of columns to assign we're done
            if col >= arr1.Length then yield path |> List.rev
            else
                // We have df "degrees of freedom" left
                for d = 1 to df + 1 do 
                    // Clock arithmetic for the row
                    let nrow = (prow + d) % arr2.Length
                    // Recurse yielding out all further paths
                    yield! inner (col + 1) nrow (df + 1 - d) 
                                 ((col, nrow) :: path)           
        }
    let res, score =
        // the difference in length 
        // is the starting "degrees of freedom"
        let diff = arr2.Length - arr1.Length
        seq {
            // for each y-axis starting point
            for y = 0 to arr2.Length - 1 do
                // 1 selected, r is the starting point (on y), 
                //    starting is always 0 on x. 
                // ((0, y) :: []) is the accumulator 
                //    with the initial (0, y) coordinates
                yield! inner 1 y diff ((0, y) :: [])
        } 
        // Sum each path to find total similarity
        |> Seq.map 
            (fun l -> l, l |> Seq.sumBy (fun (i,j) -> scores.[i,j]))
        // Get the path with the highest similarity
        |> Seq.maxBy snd
    // Create output array and copy in the results
    let out = Array.create arr1.Length Int32.MinValue
    for (i,j) in res do
        out.[i] <- j
    // Return results
    out, score
