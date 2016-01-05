open System
open System.Threading

/// Async timer to perform actions
let timer interval scheduledAction = async {
    do! interval |> Async.Sleep
    scheduledAction()
}

/// Add action to timer, return cancellation-token to cancel the action
let scheduleAction interval scheduledAction =
    let cancel = new CancellationTokenSource()
    Async.Start (timer interval scheduledAction, cancel.Token)
    cancel


// Basic idea from: http://msdn.microsoft.com/en-us/library/ee370246.aspx

/// Agent to maintain a queue of scheduled tasks that can be canceled.
/// It never runs its processor function, so it doesn't do anything.
let scheduleAgent = new MailboxProcessor<Guid * CancellationTokenSource>(fun _ -> async { () })

let postToken = scheduleAgent.Post

let cancelAction(agentId) =
    scheduleAgent.TryScan((fun (aId, source) ->
        let action =
            async {
                source.Cancel()
                return agentId
            }
        if (agentId = aId) then
            Some(action)
        else
            None), 100) // timeout: if queue is empty, wait 100ms to get a value.
    |> Async.RunSynchronously

// Testing:
// Do action when timeout:
let notifyTimeout1() = Console.WriteLine "5 seconds elapsed."
let id1 = Guid.NewGuid()
let token1 = scheduleAction 5000 notifyTimeout1
postToken(id1, token1)

// Cancel action before timeout:
let notifyTimeout2() = Console.WriteLine "This won't happen"
let id2 = Guid.NewGuid()
let token2 = scheduleAction 3000 notifyTimeout2
postToken(id2, token2)
cancelAction(id2) |> ignore
