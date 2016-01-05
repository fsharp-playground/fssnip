open System
open System.Net
open System.Collections.Generic

type Agent<'T> = MailboxProcessor<'T>

let download url = async {
  use wc = new WebClient()
  return! wc.AsyncDownloadString(Uri(url)) }

type CachingMessage =
  | Add of string * string
  | Get of string * AsyncReplyChannel<string>

let caching = Agent.Start(fun agent -> async {
  let table = Dictionary<string, string>()
  while true do
    let! msg = agent.Receive()
    match msg with
    | Add(url, html) -> 
        table.Add(url, html)
    | Get(url, repl) -> 
        if table.ContainsKey(url) then
          repl.Reply(table.[url])
        else
          repl.Reply(null)  })

let printInfo url = async {
  let! html = caching.PostAndAsyncReply(fun ch -> Get(url, ch))
  if html = null then
    let! text = download url
    caching.Post(Add(url, text))   
    printfn "Download: %s (%d)" url text.Length
  else
    printfn "Cached: %s (%d)" url html.Length }

printInfo "http://tomasp.net"
|> Async.Start