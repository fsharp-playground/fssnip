let factorial x = 
    let rec util(value, acc) = 
        match value with
        |0 | 1 -> acc
        | _    -> util(value - 1, acc * value)
    util(x,1)