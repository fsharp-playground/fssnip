let pe1_while limit = 
    let mutable acc = 0
    let mutable x = 0
    while x < limit do
        if x % 5 = 0 || x % 3 = 0 then acc <- acc + x
        x <- x + 1
    acc

pe1_while 1000