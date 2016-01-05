open System

[<AutoOpen>]
module AsyncEx = 
    type private SuccessException<'T>(value : 'T) =
        inherit Exception()
        member self.Value = value

    type Microsoft.FSharp.Control.Async with
  
        static member Choice<'T>(tasks : seq<Async<'T option>>) : Async<'T option> =
            let wrap task =
                async {
                    let! res = task
                    match res with
                    | None -> return None
                    | Some r -> return raise <| SuccessException r
                }

            async {
                try
                    do!
                        tasks
                        |> Seq.map wrap
                        |> Async.Parallel
                        |> Async.Ignore

                    return None
                with 
                | :? SuccessException<'T> as ex -> return Some ex.Value
            }

// Examples
let delay n f = 
    async {
        for i in [1..n] do
            do! Async.Sleep(1000) 
            printfn "%d %d" i n
        return f n
    }

Async.Choice [ delay 10 (fun n -> Some n); delay 20 (fun n -> Some n)]
|> Async.RunSynchronously // Some 10

Async.Choice [ delay 10 (fun n -> None); delay 20 (fun n -> Some n)]
|> Async.RunSynchronously // Some 20

Async.Choice [ delay 10 (fun n -> raise <| new Exception("oups") ); delay 20 (fun n -> Some n)]
|> Async.RunSynchronously // Exception("oups")

Async.Choice [ delay 10 (fun n -> Some n ); delay 20 (fun n -> raise <| new Exception("oups"))]
|> Async.RunSynchronously // Some 10

Async.Choice<int>[ delay 10 (fun n -> None); delay 20 (fun n -> raise <| new Exception("oups"))]
|> Async.RunSynchronously // Exception("oups")


Async.Choice<int>[ delay 10 (fun n -> raise <| new Exception("oups1")); delay 20 (fun n -> raise <| new Exception("oups2"))]
|> Async.RunSynchronously // Exception("oups1")

Async.Choice<int>[ delay 10 (fun n -> None); delay 20 (fun n -> None)]
|> Async.RunSynchronously // None