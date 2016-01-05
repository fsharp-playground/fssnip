open System.Linq

let bitcount (b:bigint) =
    let mutable b = b
    let mutable count = 0
    while b <> bigint 0 do
        b <- b &&& (b-(bigint 1))
        count <- count + 1
    count

let bitcount2 (b:System.Collections.BitArray) = 
    (b:> System.Collections.IEnumerable).Cast<bool>()
    |> Seq.map (fun v -> if v then 1 else 0) 
    |> Seq.sum

let bitcount3 (b:System.Collections.BitArray) = 
    let mutable count = 0
    for bit in b do
        if bit then
            count <- count+1
    count