open System

let randomAlphanumericString() =
    let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
    let random = new Random()
    seq {
        for i in {0..7} do
            yield chars.[random.Next(chars.Length)]
    } |> Seq.toArray |> (fun x -> new String(x))