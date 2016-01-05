open System
open System.Diagnostics
open Microsoft.FSharp.Control
open System.Threading
open System.Threading.Tasks
open System.Threading.Tasks.Dataflow
open System.Collections.Concurrent


/// Code to measure the number of messages the
/// agent can process per second on a number of threads.
let test name f g =
   let test countPerThread =
      let threadCount = System.Environment.ProcessorCount
      let msgCount = threadCount * countPerThread
      let watch = Stopwatch.StartNew()
      let incrementers =
         Array.Parallel.init threadCount (fun _ ->
            for i = 1 to countPerThread do f 100L)
      let expectedCount = int64 countPerThread * int64 threadCount * 100L 
      let finalCount = g() // expectedCount //
      watch.Stop()
      if finalCount <> expectedCount then
         failwith "Didn't work!"
      int (float msgCount / watch.Elapsed.TotalSeconds)
      
   // Warm up!
   test 10000 |> ignore<int>
   System.GC.Collect()
   // Real test
   let msgsPerSecond = test 500000
   printfn "%s processed %i msgs/sec" name msgsPerSecond


type CounterMsg =
   | Add of int64
   | GetAndReset of (int64 -> unit)

let vanillaCounter = 
   MailboxProcessor.Start <| fun inbox ->
      let rec loop count = async {
         let! msg = inbox.Receive()
         match msg with
         | Add n -> return! loop (count + n)
         | GetAndReset reply ->
            reply count
            return! loop 0L
      }
      loop 0L

//test "Vanilla Actor (MailboxProcessor)" 
//   (fun i -> vanillaCounter.Post <| Add i) 
//   (fun () -> vanillaCounter.PostAndReply(fun channel -> GetAndReset channel.Reply))



type 'a ISimpleActor =
   inherit IDisposable
   abstract Post : msg:'a -> unit
   abstract PostAndReply<'b> : msgFactory:(('b -> unit) -> 'a) -> 'b

type 'a SimpleMailbox() =
   let msgs = ConcurrentQueue<'a>()
   let onMsg = new AutoResetEvent(false)

   member __.Receive() =
      let rec await() = async {
         let mutable value = Unchecked.defaultof<_>
         let hasValue = msgs.TryDequeue(&value)
         if hasValue then return value
         else 
            let! _ = Async.AwaitWaitHandle onMsg
            return! await()        
      }
      await()

   member __.Post msg = 
      msgs.Enqueue msg
      onMsg.Set() |> ignore<bool>

   member __.PostAndReply<'b> msgFactory =
      let value = ref Unchecked.defaultof<'b>
      use onReply = new AutoResetEvent(false) 
      let msg = msgFactory (fun x ->
         value := x
         onReply.Set() |> ignore<bool>
      )
      __.Post msg
      onReply.WaitOne() |> ignore<bool>
      !value

   interface 'a ISimpleActor with
      member __.Post msg = __.Post msg
      member __.PostAndReply msgFactory = __.PostAndReply msgFactory
      member __.Dispose() = onMsg.Dispose()

module SimpleActor =
   let Start f =
      let mailbox = new SimpleMailbox<_>()
      f mailbox |> Async.Start
      mailbox :> _ ISimpleActor

let simpleActor = 
   SimpleActor.Start <| fun inbox ->
      let rec loop count = async {
         let! msg = inbox.Receive()
         match msg with
         | Add n -> return! loop (count + n)
         | GetAndReset reply ->
            reply count
            return! loop 0L
      }
      loop 0L

//test "Simple Actor" 
//   (fun i -> simpleActor.Post <| Add i) 
//   (fun () -> simpleActor.PostAndReply GetAndReset)


type 'a ISharedActor =
   abstract Post : msg:'a -> unit
   abstract PostAndReply<'b> : msgFactory:(('b -> unit) -> 'a) -> 'b

type 'a SharedMailbox() =
   let msgs = ConcurrentQueue<'a>()
   let mutable isStarted = false
   let mutable msgCount = 0
   let mutable react = Unchecked.defaultof<_>
   let mutable currentMessage = Unchecked.defaultof<_>

   let rec execute(isFirst) =

      let inline consumeAndLoop() =
         react currentMessage
         currentMessage <- Unchecked.defaultof<_>
         let newCount = Interlocked.Decrement &msgCount
         if newCount <> 0 then execute false

      if isFirst then consumeAndLoop()
      else
         let hasMessage = msgs.TryDequeue(&currentMessage)
         if hasMessage then consumeAndLoop()
         else 
            Thread.SpinWait 20
            execute false
   
   member __.Receive(callback) = 
      isStarted <- true
      react <- callback

   member __.Post msg =
      while not isStarted do Thread.SpinWait 20
      let newCount = Interlocked.Increment &msgCount
      if newCount = 1 then
         currentMessage <- msg
         // Might want to schedule this call on another thread.
         execute true
      else msgs.Enqueue msg
   
   member __.PostAndReply msgFactory =
      let value = ref Unchecked.defaultof<'b>
      use onReply = new AutoResetEvent(false)
      let msg = msgFactory (fun x ->
         value := x
         onReply.Set() |> ignore<bool>
      )
      __.Post msg
      onReply.WaitOne() |> ignore<bool>
      !value


   interface 'a ISharedActor with
      member __.Post msg = __.Post msg
      member __.PostAndReply msgFactory = __.PostAndReply msgFactory

module SharedActor =
   let Start f =
      let mailbox = new SharedMailbox<_>()
      f mailbox
      mailbox :> _ ISharedActor

let sharedActor = 
   SharedActor.Start <| fun inbox ->
      let rec loop count =
         inbox.Receive(fun msg ->
            match msg with
            | Add n -> loop (count + n)
            | GetAndReset reply ->
               reply count
               loop 0L)
      loop 0L

//test "Shared Actor" 
//   (fun i -> sharedActor.Post <| Add i) 
//   (fun () -> sharedActor.PostAndReply GetAndReset)




[<Sealed>]
type AsyncReplyChannel<'Reply> internal (replyf : 'Reply -> unit) =
    member x.Reply(reply) = replyf(reply)

[<Sealed>]
type internal AsyncResultCell<'a>() =
    let source = new TaskCompletionSource<'a>()

    member x.RegisterResult result = source.SetResult(result)

    member x.AsyncWaitResult =
        Async.FromContinuations(fun (cont,_,_) -> 
            let apply = fun (task:Task<_>) -> cont (task.Result)
            source.Task.ContinueWith(apply) |> ignore)

    member x.GetWaitHandle(timeout:int) =
        async { let waithandle = source.Task.Wait(timeout)
                return waithandle }

    member x.GrabResult() = source.Task.Result

    member x.TryWaitResultSynchronously(timeout:int) = 
        //early completion check
        if source.Task.IsCompleted then 
            Some source.Task.Result
        //now force a wait for the task to complete
        else 
            if source.Task.Wait(timeout) then 
                Some source.Task.Result
            else None

type DataflowAgent<'Msg>(initial, ?cancellationToken) =
    let cancellationToken =
        defaultArg cancellationToken Async.DefaultCancellationToken
    let mutable started = false
    let errorEvent = new Event<System.Exception>()
    let incomingMessages = new BufferBlock<'Msg>()
    let mutable defaultTimeout = Timeout.Infinite

    [<CLIEvent>]
    member this.Error = errorEvent.Publish

    member this.Start() =
        if started
            then raise (new InvalidOperationException("Already Started."))
        else
            started <- true
            let comp = async { try do! initial this
                               with error -> errorEvent.Trigger error }
            Async.Start(computation = comp, cancellationToken = cancellationToken)

    member this.Receive(?timeout) =
        Async.AwaitTask <| incomingMessages.ReceiveAsync()

    member this.Post(item) =
        let posted = incomingMessages.Post(item)
        if not posted then
            raise (InvalidOperationException("Incoming message buffer full."))

    member this.PostAndTryAsyncReply(replyChannelMsg, ?timeout) =
        let timeout = defaultArg timeout defaultTimeout
        let resultCell = AsyncResultCell<_>()
        let msg = replyChannelMsg(AsyncReplyChannel<_>(fun reply -> resultCell.RegisterResult(reply)))
        let posted = incomingMessages.Post(msg)
        if posted then
            match timeout with
            |   Threading.Timeout.Infinite ->
                    async { let! result = resultCell.AsyncWaitResult
                            return Some(result) }
            |   _ ->
                    async { let! ok =  resultCell.GetWaitHandle(timeout)
                            let res = (if ok then Some(resultCell.GrabResult()) else None)
                            return res }
        else async{return None}

    member this.PostAndAsyncReply( replyChannelMsg, ?timeout) =
            let timeout = defaultArg timeout defaultTimeout
            match timeout with
            |   Threading.Timeout.Infinite ->
                let resCell = AsyncResultCell<_>()
                let msg = replyChannelMsg (AsyncReplyChannel<_>(fun reply -> resCell.RegisterResult(reply) ))
                let posted = incomingMessages.Post(msg)
                if posted then
                    resCell.AsyncWaitResult
                else
                    raise (InvalidOperationException("Incoming message buffer full."))
            |   _ ->
                let asyncReply = this.PostAndTryAsyncReply(replyChannelMsg, timeout=timeout)
                async { let! res = asyncReply
                        match res with
                        | None ->  return! raise (TimeoutException("PostAndAsyncReply TimedOut"))
                        | Some res -> return res }

    static member Start(initial, ?cancellationToken) =
        let dfa = DataflowAgent<'Msg>(initial, ?cancellationToken = cancellationToken)
        dfa.Start()
        dfa

let dataflowAgent = DataflowAgent.Start <| fun inbox ->
      let rec loop count = async {
         let! msg = inbox.Receive()
         match msg with
         | Add n -> return! loop (count + n)
         | GetAndReset reply ->
            reply count
            return! loop 0L
      }
      loop 0L

test "DataFlow Actor" 
   (fun i -> dataflowAgent.Post <| Add i) 
   (fun () -> 
        Async.RunSynchronously(
                dataflowAgent.PostAndAsyncReply(fun channel -> GetAndReset channel.Reply) 
        )
    )


Console.ReadLine() |> ignore