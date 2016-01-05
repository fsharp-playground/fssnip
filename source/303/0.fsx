let inner_product s1 s2 =
    (s1, s2) ||> Seq.map2 (*) |> Seq.sum

let fir (kernel:double seq) (s:double seq)  =
    let k = Seq.length kernel
    s
    |> Seq.windowed k
    |> Seq.map (inner_product kernel)