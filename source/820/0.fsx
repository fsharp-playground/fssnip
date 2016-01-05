let Problem28 =
    ([3..+2..1001] |> List.map (fun x -> (x, x*x))
                   |> List.map (fun (n, s) -> let interval = n - 1
                                              let tl = s - interval
                                              let tr = s
                                              let br = s - (interval * 3)
                                              let bl = s - (interval * 2)
                                              (n, s, (tl, tr, br, bl)))
                   |> List.map (fun (_, _, (tl, tr, br, bl)) -> tl + tr + br + bl)
                   |> List.sum) + 1