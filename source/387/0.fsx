open System
open System.IO
open System.Net
open System.Threading
open System.Collections.Generic

/// Type alias for the MailboxProcessor type
type Agent<'T> = MailboxProcessor<'T>

// System.Net Extensions
type System.Net.HttpListener with
  /// Asynchronously retrieves the next incoming request
  member x.AsyncGetContext() = 
    Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)

type System.Net.WebClient with
  /// Asynchronously downloads data from the 
  member x.AsyncDownloadData(uri) = 
    Async.FromContinuations(fun (cont, econt, ccont) ->
      x.DownloadDataCompleted.Add(fun res ->
        if res.Error <> null then econt res.Error
        elif res.Cancelled then ccont (new OperationCanceledException())
        else cont res.Result)
      x.DownloadDataAsync(uri) )

type System.Net.HttpListener with 
  /// Starts an HTTP server on the specified URL with the
  /// specified asynchronous function for handling requests
  static member Start(url, f) = 
    let tokenSource = new CancellationTokenSource()
    Async.Start
      ( ( async { 
            use listener = new HttpListener()
            listener.Prefixes.Add(url)
            listener.Start()
            while true do 
              let! context = listener.AsyncGetContext()
              Async.Start(f context, tokenSource.Token) }),
        cancellationToken = tokenSource.Token)
    tokenSource

  /// Starts an HTTP server on the specified URL with the
  /// specified synchronous function for handling requests
  static member StartSynchronous(url, f) =
    HttpListener.Start(url, f >> async.Return) 

// [snippet:Common functions and declarations]
// NOTE: This snippet uses System.Net extensions from: http://fssnip.net/6d
// (such as HttpListener.Start and WebClient.AsyncDownloadData)

// Location where the proxy copies content from
let root = "http://msdn.microsoft.com"

// Maps requests from local URL to target URL
let getProxyUrl (ctx:HttpListenerContext) = 
  Uri(root + ctx.Request.Url.PathAndQuery)

// Handle exception asynchronously - generate page with message
let asyncHandleError (ctx:HttpListenerContext) (e:exn) = async {
  use wr = new StreamWriter(ctx.Response.OutputStream)
  wr.Write("<h1>Request Failed</h1>")
  wr.Write("<p>" + e.Message + "</p>")
  ctx.Response.Close() }
// [/snippet]


module Chunked = 

  // [snippet:Extension #1: Chunked proxy server]
  /// Handles request using asynchronous workflows - The content
  /// is downloaded and sent to the caller in chunks, so the proxy
  /// is more efficient and doesn't read entire file in memory
  let asyncHandleRequest (ctx:HttpListenerContext) = async {
    // Initialize HTTP connection to the server
    let request = HttpWebRequest.Create(getProxyUrl(ctx))
    use! response = request.AsyncGetResponse()
    use stream = response.GetResponseStream()
    ctx.Response.SendChunked <- true

    // Asynchronous loop to copy data
    let count = ref 1
    let buffer = Array.zeroCreate 4096
    while count.Value > 0 do
      let! read = stream.AsyncRead(buffer, 0, buffer.Length)
      do! ctx.Response.OutputStream.AsyncWrite(buffer, 0, read)    
      count := read
    ctx.Response.Close() }

  // Start HTTP proxy that handles requests asynchronously using chunking
  let token = HttpListener.Start("http://localhost:8080/", asyncHandleRequest)
  token.Cancel()
  // [/snippet]

module Cached = 
  
  // [snippet:Extension #2: Agent-based in-memory cache (Part 1)]
  /// The cache supports messages for retrieving and adding content
  type CacheMessage =
    | TryGet of Uri * AsyncReplyChannel<option<byte[]>>
    | Add of Uri * byte[]

  /// Represents a thread-safe in-memory cache for web pages
  let cache = Agent.Start(fun agent -> async {
    // Store cached pages in a mutable dictionary
    let pages = new Dictionary<_, _>()
    while true do
      let! msg = agent.Receive()
      match msg with 
      | TryGet(url, repl) ->
          // Try to return page from the cache
          match pages.TryGetValue(url) with
          | true, data -> repl.Reply(Some(data))
          | _ -> repl.Reply(None)
      | Add(url, data) ->
          // Add downloaded page to the cache
          pages.[url] <- data })
  // [/snippet]

  // [snippet:Extension #2: Proxy server using cache (Part 2)]
  /// Attempts to retrieve page from cache first. If the page 
  /// isn't cached already, download it and add it to the cache.
  let downloadUsingCache (url:Uri) = async {
    let! cached = cache.PostAndAsyncReply(fun ch -> TryGet(url, ch))
    match cached with 
    | Some(data) -> 
        // Return page from the cache
        return data
    | _ ->
        // Download page and add it to the cache
        let wc = new WebClient()
        let! data = wc.AsyncDownloadData(url)
        cache.Post(Add(url, data))
        return data }

  /// Handle proxy request using cache - Obtain the page content
  /// using helper function and send it to the output stream.
  /// Exceptions can still be handled easily using try ... with.
  let asyncHandleRequest (ctx:HttpListenerContext) = async {
    try
      let! data = downloadUsingCache (getProxyUrl ctx)
      do! ctx.Response.OutputStream.AsyncWrite(data)
      ctx.Response.Close() 
    with err ->
      do! asyncHandleError ctx err }

  // Start HTTP proxy that handles requests asynchronously using cache
  let token = HttpListener.Start("http://localhost:8080/", asyncHandleRequest)
  token.Cancel()
  // [/snippet]
