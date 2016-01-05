open System
open Hopac

type Expiring =
    { Complete : unit -> unit }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Expiring =
    let create name minTime windowTime =
        let completeCh = Ch.Now.create()
        let expire =
            job {
                let! stage1 =
                    Alt.choose [
                        Ch.take completeCh |> Alt.map (fun () -> printfn "%s: early" name; Choice1Of2 ())
                        Timer.Global.timeOut minTime |> Alt.map (fun () -> Choice2Of2 ())
                    ]
                match stage1 with
                | Choice1Of2 () -> return ()
                | Choice2Of2 () ->
                    return!
                        Alt.choose [
                            Ch.take completeCh |> Alt.map (fun () -> printfn "%s: in time" name)
                            Timer.Global.timeOut windowTime |> Alt.map (fun () -> printfn "%s: late" name)
                        ]
            }
        run <| Job.start expire
        { Complete = fun () -> run <| Ch.give completeCh () }

let e1 = Expiring.create "e1" (TimeSpan.FromSeconds 1.) (TimeSpan.FromSeconds 1.)
let e2 = Expiring.create "e2" (TimeSpan.FromSeconds 1.) (TimeSpan.FromSeconds 1.)
let e3 = Expiring.create "e3" (TimeSpan.FromSeconds 1.) (TimeSpan.FromSeconds 1.)

printfn "Completing e1 early!"
e1.Complete ()

Async.RunSynchronously <| Async.Sleep 1500
printfn "Completing e2 on time!"
e2.Complete ()

printfn "Waiting for e3 to time out..."

Console.ReadLine() |> ignore
