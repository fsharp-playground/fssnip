let movingAverage (period : int) (values : float seq) =
    Seq.windowed period values
    |> Seq.map (fun x -> Array.average x)