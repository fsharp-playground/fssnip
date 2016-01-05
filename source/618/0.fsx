open System.Threading

type Message =
| Add of int
| Get of AsyncReplyChannel<int>
| Quit


let boxi = MailboxProcessor.Start(fun inbox ->
  let startTime = System.DateTime.Now.Ticks
  let rec loop( msgordinal, n ) =
    async {
      
     
      let! msg = inbox.Receive()
      match msg with
        | Get(replyChannel) ->
            replyChannel.Reply( n )
            return! loop( msgordinal + 1, n )
        | Add(x) ->
            async { 
              do! Async.Sleep 2000
              printfn "%d woke up" msgordinal
            } |> Async.Start
            printfn "Number of messages STILL in the queue: %d" inbox.CurrentQueueLength      
            let span = System.TimeSpan(System.DateTime.Now.Ticks - startTime)
            printfn "Processing message %d in thread %d, %s after start of this MailboxProcessor" msgordinal Thread.CurrentThread.ManagedThreadId (span.ToString("G"))
            return! loop( msgordinal + 1, n+x )
        | Quit -> return ( )
    }
  loop( 0, 0 )
)

[1 .. 10] |> List.iter( fun x -> boxi.Post( Add x ))
printfn "And now start some serious waiting for results"
let count = boxi.PostAndReply(fun replyChannel -> Get( replyChannel) )
printfn "And (finally). we got response: %d" count