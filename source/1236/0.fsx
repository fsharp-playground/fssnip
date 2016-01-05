let columnNum =
    let charValues =
        ['A'..'Z']
        |> Seq.mapi (fun i c -> c, i + 1)
        |> Map.ofSeq

    fun (code: string) ->
        (Seq.toList code, (0,-1))
        ||> List.foldBack(fun v (depth, total) -> depth + 1, total + (pown 26 depth * charValues.[v]))
        |> snd