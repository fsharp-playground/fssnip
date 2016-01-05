let movingAverage (period : int) (values : float seq) =
    Seq.zip values (Seq.skip period values)
    |> Seq.scan (fun last (prev, cur) -> last - prev + cur) (values |> Seq.take period |> Seq.sum)
    |> Seq.map (fun x -> x / float(period))