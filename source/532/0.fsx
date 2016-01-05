let IntAndString value =    
    let (|Match|_|) pattern input =
        let m = Regex.Match(input, pattern) in
        if m.Success then Some ([ for g in m.Groups -> g.Value ]) else None
    match value with
        | Match @"((?>\d+))(\w+)" x -> Some(x)              
        | Match @"((?>\d+))" x      -> Some(x @ ["items"])  
        | Match @"(\w+)" x          -> Some(x)             
        | _                         -> None