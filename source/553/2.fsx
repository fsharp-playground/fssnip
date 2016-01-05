open System
open System.Net
open System.Collections.Generic
open Microsoft.FSharp.Control.WebExtensions

// [snippet:Caching agent implementation]
type Agent<'T> = MailboxProcessor<'T>

/// The message type - 'Add' message adds a newly downloaded
/// page to the cache, 'Get' retreives the cached value and
/// 'Clear' deletes all cached pages.
type CachingMessage =
  | Add of string * string
  | Get of string * AsyncReplyChannel<option<string>>
  | Clear

/// Caching agent - keeps a mutable .NET dictionary containing
/// key-value pairs with URL and the cached HTML data
let caching = Agent.Start(fun agent -> async {
  let table = Dictionary<string, string>()
  while true do
    let! msg = agent.Receive()
    match msg with
    | Add(url, html) -> 
        // Add downloaded page to the cache
        table.Add(url, html)
    | Get(url, repl) -> 
        // Get a page from the cache - returns 
        // None if the value isn't in the cache
        if table.ContainsKey(url) then
          repl.Reply(Some table.[url])
        else
          repl.Reply(None) 
    | Clear ->
        table.Clear() })
// [/snippet]

// [snippet:Example: Downloading web pages]
/// Prints information about the specified web site using cache
let printInfo url = async {
  // Try to get the cached HTML from the caching agent
  let! htmlOpt = caching.PostAndAsyncReply(fun ch -> Get(url, ch))
  match htmlOpt with
  | None ->
      // New url - download it and add it to the cache
      use wc = new WebClient()
      let! text = wc.AsyncDownloadString(Uri(url))
      caching.Post(Add(url, text))   
      printfn "Download: %s (%d)" url text.Length
  | Some html ->
      // The url was downloaded earlier 
      printfn "Cached: %s (%d)" url html.Length }

// Print information about a web site -
// Run this repeatedly to use cached value
printInfo "http://functional-programming.net"
|> Async.Start

// Clear the cache - 'printInfo' will need to
// download data from the web site again
caching.Post(Clear)
// [/snippet]