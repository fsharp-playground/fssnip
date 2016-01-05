let (|Array|_|) pattern toMatch =
    let patternLength = Array.length pattern
    let toMatchLength = Array.length toMatch
    let tail = Array.sub toMatch patternLength (toMatchLength - patternLength)
    let completePattern = Array.concat [pattern ; tail ]
    if toMatch = completePattern then 
        Some(tail)
    else 
        None
        
let toMatch = [|1;2;3|]
let pattern = [|1|]

match toMatch with | Array pattern tail -> sprintf "bingo %i" (tail |> Array.sum)
;;