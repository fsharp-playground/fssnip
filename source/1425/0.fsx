let toBase26 num = 
    let digits = 'Z' :: ['A' .. 'Y']
    let rec conv lst x =
        match x with
        | 0 -> lst
        | _ ->  digits.[x % digits.Length] :: conv lst (x / digits.Length)
    
    new System.String(num |> conv [] |> List.rev |> List.toArray)
