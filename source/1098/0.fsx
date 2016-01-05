open FSharp.Control

let inline (<--) (agent: ^a) (msg: 'b) =
    (^a: (member Post: 'b -> unit) (agent, msg))

let test =
    let mailboxProcessor = MailboxProcessor.Start(fun inbox -> async { return () }) 
    let autoCancelAgent = AutoCancelAgent.Start(fun inbox -> async { return () })

    mailboxProcessor <-- ()
    autoCancelAgent <-- ()