type JobMessage = 
    DoJob of Async<unit>
    | DoneJob
    | Quit of AsyncReplyChannel<unit>

let Worker () = 
    MailboxProcessor<JobMessage>.Start(fun mbox -> 
        let rec acceptingPhase pending  = 
            async {
                let! m = mbox.Receive() 
                match m with
                | DoJob j -> async { do! j 
                                     mbox.Post(DoneJob) 
                                   } |> Async.Start 
                             do! acceptingPhase (pending+1)
                | DoneJob -> do! acceptingPhase (pending-1)
                | Quit reply -> do! quitPhase reply pending
            }
        and quitPhase reply pending = 
            async {
                if pending <= 0 then reply.Reply()
                else
                    let! m = mbox.Receive() 
                    match m with 
                    | DoneJob -> do! quitPhase reply (pending-1)
                    | _ -> do! quitPhase reply pending
            }
        acceptingPhase 0
        )