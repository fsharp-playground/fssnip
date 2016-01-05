let windowAt str offset radius =
    let startAt = max 0 (offset - radius)
    let endAt = min (offset + radius) (String.length str - 1)  
    [ for i in startAt .. endAt -> str.[i] ]

let jaro s1 s2 = 
    let matchRadius = 
        let s1_l, s2_l = String.length s1, String.length s2 in
            (min s1_l s2_l) / 2 +
            (min s1_l s2_l) % 2

    let commonChars chars1 chars2 =
        [
            for i = 0 to String.length chars1 - 1 do
                let matchChar = chars1.[i]
                let windowChars = windowAt chars2 i matchRadius
                if windowChars |> List.exists (fun c -> c = matchChar) then
                    yield matchChar 
        ]
            
    let common1 = commonChars s1 s2
    let common2 = commonChars s2 s1

    let totalTranspositions = 
        List.zip common1 common2
        |> List.fold (fun s (c1,c2) -> if c1 <> c2 then s + 1.0 else s) 0.0

    let transpositions = totalTranspositions / 2.0
    let s1length = float (String.length s1)
    let s2length = float (String.length s2)
    let c1length = float (List.length common1)    
    let c2length = float (List.length common2)

    ((c1length / s1length) + 
     (c2length / s2length) + 
     ((c1length - transpositions) / c1length)) 
     / 3.0


let jaroWinkler s1 s2 = 
    let jaroScore = jaro s1 s2
    let p = 0.1
    let maxLength = (min s1.Length s2.Length) - 1
    let rec calcL i acc =
        if i > maxLength || s1.[i] <> s2.[i] then acc
        else calcL (i + 1) (acc + 1.0)
    let l = calcL 0 0.0
    jaroScore + (l * p * (1.0 - jaroScore))