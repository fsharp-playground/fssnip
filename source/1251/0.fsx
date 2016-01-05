open System

let intersperseLast lastDelim delim list =  
    let length = Seq.length list
    Seq.scan (fun acc elem ->
                match acc with 
                | (count, state) when count < length - 1 -> (count + 1, delim::elem::state)
                | (count, state) when count = length -> (count + 1, elem::state)
                | (count, state) -> (count + 1, lastDelim::elem::state)) (1, []) list
        |> Seq.last
        |> snd        
        |> Seq.toList
        |> List.rev



let intersperseLastList lastDelim delim list = 
    let length = List.length list
    List.foldBack(fun elem acc ->
        match acc with 
                | (count, state) when count = 1 -> (count - 1, elem::state)
                | (count, state) when count = length -> (count - 1, lastDelim::elem::state)
                | (count, state) -> (count - 1, delim::elem::state)) list (length, [])