query {
    for row in Players do
    let key = AnonymousObject<_,_>(row.NameFirst, row.NameLast)
    groupValBy row key into grouping
    select (grouping.Key.Item1, grouping.Key.Item2, grouping.Count())
} |> Seq.iter (fun row -> printfn "%A" row)