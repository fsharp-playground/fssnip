type WaitCancellationResult = Signalled | TimedOut | Cancelled
type Microsoft.FSharp.Control.Async with
    static member AwaitWaitHandleWithCancellation(h: System.Threading.WaitHandle,
                                                    ct: System.Threading.CancellationToken,
                                                    ?opt_timeout : int)
                                                    : Async<WaitCancellationResult> =
        let waiter = match opt_timeout with
                        | Some timeout -> Async.AwaitWaitHandle(h, timeout)
                        | None -> Async.AwaitWaitHandle(h)
        Async.FromContinuations (fun (cont, econt, ccont) ->
            Async.StartWithContinuations(
                    waiter,
                    (fun r -> cont <| if r then Signalled else TimedOut),
                    econt,
                    (fun c -> cont <| if ct.IsCancellationRequested then Cancelled else TimedOut),
                    ct) )