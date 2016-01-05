open System
open System.Threading

module LazyFuture =
    let ofAsync eager' (computation:Async<'a>) =
        let result = ref Unchecked.defaultof<_>
        let gate = new ManualResetEventSlim (false)
        
        let init =
            async { let! res = computation |> Async.Catch
                    result := res
                    gate.Set () }

        if eager' then init |> Async.Start

        lazy
            if not (eager') then init |> Async.Start
            gate.Wait ()
            match !result with
            | Choice1Of2 r -> r
            | Choice2Of2 e -> raise e

    let create eager' computation = ofAsync eager' (async { return computation () })

let v = LazyFuture.create false (fun () -> Thread.Sleep(3000); 10) 
v.Force () |> printfn "%A"