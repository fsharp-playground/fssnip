open System

let fib () =
    seq {
        yield 1L
        yield 2L
        let rec loop last =
            seq {
                let next = fst last + snd last
                yield next
                yield! loop (snd last, next)
            }
        yield! loop (1L,2L)
    }

let createSamples () =
    fib ()

[<EntryPoint>]
let main argv = 
    let samples = createSamples ()
    samples |> Seq.take 50 |> Array.ofSeq |> printfn "%A"
    0 // return an integer exit code