let chunk chunkSize (arr : _ array) = 
    query {
        for idx in 0..(arr.Length - 1) do
        groupBy (idx / chunkSize) into g
        select (g |> Seq.map (fun idx -> arr.[idx]))
    }