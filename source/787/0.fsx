module Seq =

    /// Skip the first n lines of the sequence, and don't throw an exception if there are fewer than n lines.
    let skipIf n s =
        s
        |> Seq.mapi (fun i elem -> i, elem)
        |> Seq.choose (fun (i, elem) -> if i >= n then Some(elem) else None)

module Examples = 

    let skipdemo01 =
        Seq.init 0 (fun x -> x)
        |> Seq.skipIf 1

    let skipdemo11 =
        Seq.init 1 (fun x -> x)
        |> Seq.skipIf 1

    let skipdemo02 =
        Seq.init 0 (fun x -> x)
        |> Seq.skipIf 2

    let skipdemo32 =
        Seq.init 3 (fun x -> x)
        |> Seq.skipIf 2

    let skipdemo30 =
        Seq.init 3 (fun x -> x)
        |> Seq.skipIf 0