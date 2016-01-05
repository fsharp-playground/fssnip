let inline mySumBy (projection : ^T -> ^U) (source : seq< ^T >) : ^U =
    (LanguagePrimitives.GenericZero< ^U >, source)
    ||> Seq.fold (fun acc value -> value |> projection |> Checked.(+) acc)

do
    let l = [ 1, 11.; 2, 22.; 3, 33.; 4, 44. ]
    let ints = mySumBy fst l
    let floats = mySumBy snd l
    printfn "ints: %d, floats: %f" ints floats