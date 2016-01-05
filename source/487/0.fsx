let jaroWinklerMI (t1:string) (t2:string) = 
    // Optimizations for easy to calculate cases
    if t1.Length = 0 || t2.Length = 0 then 0.0
    elif t1 = t2 then 1.0
    else
        // Even more weight for the first char
        let score = jaroWinkler t1 t2
        let p = 0.2 //percentage of score from new metric
        let b = if t1.[0] = t2.[0] then 1.0 else 0.0
        ((1.0 - p) * score) + (p * b)

let scoreNamePairs (t1:string) (t2:string) =  
    //Raise jaro to a power in order to over-weight better matches        
    jaroWinklerMI t1 t2 ** 2.0