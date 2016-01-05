let histogram =
    Seq.fold (fun acc key ->
        if Map.containsKey key acc
        then Map.add key (acc.[key] + 1) acc
        else Map.add key 1 acc
    ) Map.empty
    >> Seq.sortBy (fun kvp -> -kvp.Value)

"Testing shows the presence, not the absence of bugs"
|> histogram |> Seq.iter (printfn "%A")
