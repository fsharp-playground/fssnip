// [snippet:Definition of HandlingMailbox]
/// A wrapper for MailboxProcessor that catches all unhandled exceptions
/// and reports them via the 'OnError' event. Otherwise, the API
/// is the same as the API of 'MailboxProcessor'
type HandlingMailbox<'T>(f:HandlingMailbox<'T> -> Async<unit>) as self =
  // Create an event for reporting errors
  let errorEvent = Event<_>()
  // Start standard MailboxProcessor 
  let inbox = new MailboxProcessor<_>(fun inbox -> async {
    // Run the user-provided function & handle exceptions
    try return! f self
    with e -> errorEvent.Trigger(e) })
  /// Triggered when an unhandled exception occurs
  member x.OnError = errorEvent.Publish
  /// Starts the mailbox processor
  member x.Start() = inbox.Start()
  /// Receive a message from the mailbox processor
  member x.Receive() = inbox.Receive()
  /// Post a message to the mailbox processor
  member x.Post(v:'T) = inbox.Post(v)
  /// Start the mailbox processor
  static member Start(f) =
    let mbox = new HandlingMailbox<_>(f)
    mbox.Start()
    mbox
// [/snippet]

// [snippet:Example of use]
// The usage is the same as with standard MailboxProcessor
let counter = HandlingMailbox<_>.Start(fun inbox -> async {
  while true do 
    printfn "waiting for data..." 
    let! data = inbox.Receive()
    // Simulate an exception 
    failwith "fail!" })

// Specify callback for unhandled errors & send message to mailbox
counter.OnError.Add(printfn "Exception: %A")
counter.Post(42) 
// [/snippet]