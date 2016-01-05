open System.Collections.Generic

/// The agent handles two kind of messages - the 'Start' message is sent
/// when the caller wants to start a new work item. The 'Finished' message
/// is sent (by the agent itself) when one work item is completed.
type LimitAgentMessage = 
  | Start of Async<unit>
  | Finished

/// A function that takes the limit - the maximal number of operations it
/// will run in parallel - and returns an agent that accepts new
/// tasks via the 'Start' message 
let threadingLimitAgent limit = MailboxProcessor.Start(fun inbox -> async {
  // Keep number of items running & queue of items to run later
  // NOTE: We keep an explicit queue, so that we can e.g. start dropping 
  // items if there are too many requests (or do something else)
  // NOTE: The loop is only accessed from one thread at each time
  // so we can just use non-thread-safe queue & mutation
  let queue = Queue<_>()
  let count = ref 0
  while true do
    let! msg = inbox.Receive() 
    // When we receive Start, add the work to the queue
    // When we receive Finished, do count--
    match msg with 
    | Start work -> queue.Enqueue(work)
    | Finished -> decr count
    // After something happened, we check if we can
    // start a next task from the queue
    if count.Value < limit && queue.Count > 0 then
      incr count
      let work = queue.Dequeue()
      // Start it in a thread pool (on background)
      Async.Start(async { 
        do! work
        inbox.Post(Finished) }) })

// Create an agent that can run at most 2 tasks in parallel
// and send 10 work items that take 1 second to the queue 
let agent = threadingLimitAgent 2
for i in 0 .. 10 do
  agent.Post(Start(async {
    do! Async.Sleep(1000)
    printfn "Finished: %d" i
  }))