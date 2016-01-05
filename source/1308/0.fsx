module Console

// simple way to synchronize writes to the console
// just post the message string to this agent

let Agent = 
    MailboxProcessor<string>.Start(fun inbox ->        
        async { while true do
                    let! msg = inbox.Receive()
                    System.Console.WriteLine(msg)})