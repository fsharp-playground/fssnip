namespace ActorModel
open System.Threading
open System

type ActorReplyChannel<'Reply>(replyf: 'Reply->unit) =
    member x.Reply(reply) = replyf(reply)

type ActorMailbox<'a > () =
    let queue = System.Collections.Generic.Queue<'a>()
    let gotMsg = new AutoResetEvent(false)
    let mutable currentLength = 0

    let incr() = Interlocked.Increment(&currentLength) |> ignore
    let decr() = Interlocked.Decrement(&currentLength) |> ignore

    let dequeue() = 
        let v = lock queue (fun () -> queue.Dequeue())
        decr()
        v

    let enqueue v = 
        lock queue (fun () -> queue.Enqueue(v))
        incr()
        gotMsg.Set() |> ignore
        let sp = new System.Threading.SpinWait()
        sp.SpinOnce()
          
    member x.Receive() =
        let rec loop() = 
            async {
                let sp = new SpinWait()
                sp.SpinOnce()
                if currentLength > 0 then
                    let v = dequeue()
                    return v
                else
                    let! b = Async.AwaitWaitHandle gotMsg
                    return! loop()}
        loop()

    member x.Post msg = enqueue msg

    member x.CurrentQueueLength = currentLength

    member x.PostAndReply (mConstructor, ?timeout:int) = 
        let timeout = defaultArg timeout Timeout.Infinite
        let v = ref Unchecked.defaultof<_>
        use gotReply = new ManualResetEvent(false)
        let msg = mConstructor (new ActorReplyChannel<_>(fun reply ->
            v := reply
            gotReply.Set() |> ignore))
        x.Post(msg) 
        match timeout with
        | Timeout.Infinite ->
            gotReply.WaitOne() |> ignore
            !v
        | _ ->
            let ok = gotReply.WaitOne(timeout)
            if ok then !v 
            else raise (TimeoutException("actor timed out"))

    member x.PostAndAsyncReply (mConstructor, ? timeout:int) =
        let timeout = defaultArg timeout Timeout.Infinite
        let v = ref Unchecked.defaultof<_>
        let gotReply = new ManualResetEvent(false)
        let msg = mConstructor (new ActorReplyChannel<_>(fun reply ->
            v := reply
            gotReply.Set() |> ignore))
        x.Post(msg) 
        match timeout with
        | Timeout.Infinite ->
            async {
                let! _ = Async.AwaitWaitHandle(gotReply)
                gotReply.Dispose()
                return !v }
        | _ ->
            async {
                let! ok = Async.AwaitWaitHandle(gotReply, timeout)
                gotReply.Dispose()
                if ok then return !v 
                else return! raise (TimeoutException("actor timed out"))}

    static member Start(f,t) =
        let mailbox = new ActorMailbox<'a>()
        Async.Start(f mailbox,t)
        mailbox //:> ActorMailbox<'a>

    interface IDisposable with
        member x.Dispose() = gotMsg.Dispose()


           