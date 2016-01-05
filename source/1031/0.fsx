let bananaSplit x =
    let first = seq [ Seq.head x; Seq.head <| Seq.skip 1 x ]
    let second = Seq.skip 2 x
    (first, second)
