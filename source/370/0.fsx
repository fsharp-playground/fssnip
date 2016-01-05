let repeatUntilSome f =
    Seq.initInfinite (fun _ -> f())
    |> Seq.find Option.isSome
    |> Option.get

let repeatUntilTrue f =
    Seq.initInfinite (fun _ -> f())
    |> Seq.find id
    |> ignore