module Seq =
    let unsort xs =
        let rand = System.Random(Seed=0)
        xs
        |> Seq.map (fun x -> rand.Next(),x)
        |> Seq.cache
        |> Seq.sortBy fst
        |> Seq.map snd