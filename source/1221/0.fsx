let bitcount (b:bigint) =
    let mutable b = b
    let mutable count = 0
    while b <> bigint 0 do
        b <- b &&& (b-(bigint 1))
        count <- count + 1
    count