open System

let start (ag : MailboxProcessor<_>) = ag.Start(); ag

type Msg =
    | Start of DateTime
    | Step
    | End of AsyncReplyChannel<float>

let lastNode =
    new MailboxProcessor<_>(fun inbox ->
        let rec run time = async {
            let! m = inbox.Receive()
            match m with
            | Start t -> do! run t
            | Step -> do! run time
            | End chan->
                chan.Reply (DateTime.Now - time).TotalSeconds
                do! run DateTime.Now
        }
        run DateTime.Now)
    |> start

let getPrevAgent (agent : MailboxProcessor<_>) =
    new MailboxProcessor<_>(fun inbox -> async {
                while true do
                    let! m = inbox.Receive()
                    agent.Post m
            })
    |> start

let firstNode = Seq.unfold (fun s -> Some(s, getPrevAgent s)) lastNode |> Seq.nth 100

let nRoundTrip n =
    firstNode.Post (Start DateTime.Now)
    for x = 1 to (n - 2) do
        firstNode.Post Step
    firstNode.PostAndReply(fun chan -> End chan);;

nRoundTrip 10000