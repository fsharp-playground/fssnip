open System

type Agent<'T> = MailboxProcessor<'T>

type MailboxProcessor<'T> with
    member agent.PostAndAsyncReplyFailable (failureEvent : IObservable<exn>) buildMessage = async {
        let! token = Async.CancellationToken // capture the current cancellation token
        return! Async.FromContinuations(fun (cont, econt, ccont) ->
            // start an agent which will wait for a message indicating which continuation to call
            let continuator = Agent.Start((fun (mailbox : Agent<Choice<'Reply, exn, OperationCanceledException>>) ->
                async {
                    // if the cancellation token is canceled or the agent fails, post the appropriate message
                    use __ = token.Register((fun _ ->
                        let result = Choice3Of3 (new OperationCanceledException("The opeartion was cancelled."))
                        mailbox.Post result))
                    use __ = failureEvent.Subscribe(fun exn -> mailbox.Post(Choice2Of3 exn))

                    // wait for a single message and call the appropriate continuation
                    let! message = mailbox.Receive()
                    match message with
                    | Choice1Of3 reply -> cont reply
                    | Choice2Of3 exn -> econt exn
                    | Choice3Of3 exn -> ccont exn }))
            
            // start another async wokrflow which will post a message to the agent and wait for a response, then
            // forward it to the continuator agent
            Async.Start( async {
                let! reply = agent.PostAndAsyncReply buildMessage
                continuator.Post(Choice1Of3 reply) }, token)) }

// example: randomly failing agent 
type RandomlyFailingAgent(failureProbability, responseDelay) =
    let agentFailure = new Event<exn>() // event fired when an error occurs

    // start an agent mailbox which
    let agent = Agent.Start(fun (mailbox : Agent<AsyncReplyChannel<unit>>) ->
        
        // returns true in case of failure and false otherwise
        let failRandomly =
            let gen = new Random()
            (fun () -> gen.NextDouble() < failureProbability)
    
        // message processing loop
        let rec loop () = async {
            let! message = mailbox.Receive()
            do! Async.Sleep responseDelay
            
            if failRandomly() then // if a failure occurs then go to the failed state
                return! failed (new Exception("Agent died unexpectedly."))
            else // otherwise reply to the message and keep processing
                message.Reply()
                return! loop () }

        // failed loop just triggers the failur event so that the error continuation is called for new messages
        and failed exn = async {
            agentFailure.Trigger exn
            let! __ = mailbox.Receive()
            return! failed exn }
        
        loop ())

    // post a failable message to the agent mailbox
    member __.MakeRequestAsync() =
        (fun replyChannel -> replyChannel)
        |> agent.PostAndAsyncReplyFailable (agentFailure.Publish)

[<EntryPoint>]
let main _ = 
    let cancellationCapability = new System.Threading.CancellationTokenSource()
    let unreliable = new RandomlyFailingAgent(0.01, 10) // create an unreliable agent
    
    // define an asynchronous workflow which will keep posting messages until one fails
    let rec loop n = async {
        do! unreliable.MakeRequestAsync()
        printfn "Successfully completed %d requests." n
        do! loop (n + 1) }
    
    Async.StartWithContinuations((loop 1),
        (ignore), // computation will run until failure or cancellation
        (fun exn -> printfn "Failed due to error: %A." exn.Message), 
        (fun exn -> printfn "Canceled: %A." exn.Message),
        cancellationCapability.Token)

    // press enter to cancel the cancellation capability
    Console.ReadLine() |> ignore
    cancellationCapability.Cancel()
    
    // make one more request to see if the agent is still alive
    Async.StartWithContinuations(unreliable.MakeRequestAsync(),
        (fun () -> printfn "Successfully made a follow-up request. Agent is still alive and kicking."),
        (fun exn -> printfn "Failed: %s" exn.Message),
        ignore)

    Console.ReadLine() |> ignore
    0 // return an integer exit code
