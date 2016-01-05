let inline min3 one two three = 
    if one < two && one < three then one
    elif two < three then two
    else three

let wagnerFischer (s: string) (t: string) =
    let m = s.Length
    let n = t.Length
    let d = Array2D.create (m + 1) (n + 1) 0

    for i = 0 to m do d.[i, 0] <- i
    for j = 0 to n do d.[0, j] <- j    

    for j = 1 to n do
        for i = 1 to m do
            if s.[i-1] = t.[j-1] then
                d.[i, j] <- d.[i-1, j-1]   // no change
            else
                d.[i, j] <-
                    min3
                        (d.[i-1, j  ] + 1) // a deletion
                        (d.[i  , j-1] + 1) // an insertion
                        (d.[i-1, j-1] + 1) // a substitution
    d.[m,n]

let damerauLevenshtein (s: string) (t: string) =
    let m = s.Length
    let n = t.Length
    let d = Array2D.create (m + 1) (n + 1) 0

    for i = 0 to m do d.[i, 0] <- i
    for j = 0 to n do d.[0, j] <- j    

    for j = 1 to n do
        for i = 1 to m do
            let cost = if s.[i-1] = t.[j-1] then 0 else 1
            d.[i, j] <-
                min3
                    (d.[i-1, j  ] + 1) // a deletion
                    (d.[i  , j-1] + 1) // an insertion
                    (d.[i-1, j-1] + cost) // a substitution or null move
            if i > 1 && j > 1 && s.[i-1] = t.[j-2] && s.[i-2] = t.[j-1] then
                d.[i, j] <- 
                    min d.[i,j] (d.[i-2, j-2] + cost) // a transposition
    d.[m,n]