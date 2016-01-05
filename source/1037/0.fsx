let keys: Map<'Key, 'T> -> seq<'Key> = Map.toSeq >> Seq.map fst

let sm = ["1", "2"] |> Map.ofSeq |> keys
let im = [1, 1] |> Map.ofSeq |> keys