// [snippet:Discriminated union representing two messages]
/// Discriminated union representing two messages - one for sending
/// numbers to the agent and one for resetting the state of the agent
type CounterMessage = 
  | Update of float
  | Reset
// [/snippet]


// [snippet:Simple agent that calculates average and can be reset]
// Simple agent that calculates average and can be reset
let counter = MailboxProcessor.Start(fun agent -> 

  // Function that implements the body of the agent
  let rec loop sum count = async {
    // Asynchronously wait for the next message
    let! msg = agent.Receive()
    match msg with 
    | Reset -> 
        // Restart loop with initial values
        return! loop 0.0 0.0

    | Update f -> 
        // Update the state and print the statistics
        let sum, count = sum + f, count + 1.0
        printfn "Average: %f" (sum / count)

        // Wait before handling the next message
        do! Async.Sleep(1000)
        return! loop sum count }
  
  // Start the body with initial values
  loop 0.0 0.0)
// [/snippet]

// [snippet:Testing the agent interactively]
// Test the bahaviour of the agent by sending the following
// messages to the agent (works via Try F# website)
counter.Post(Update 10.0)
counter.Post(Update 5.0)
counter.Post(Update 10.0)
counter.Post(Reset)
counter.Post(Update 10.0)
counter.Post(Update 5.0)
// [/snippet]