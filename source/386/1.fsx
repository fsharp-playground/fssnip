open System
open System.IO
open System.Net
open System.Threading
open System.Collections.Generic

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
// Location where the proxy copies content from
let root = "http://msdn.microsoft.com"

// Maps requests from local URL to target URL
let getProxyUrl (ctx:HttpListenerContext) = 
  Uri(root + ctx.Request.Url.PathAndQuery)

// Handle exception - generate page with message
let handleError (ctx:HttpListenerContext) (e:exn) =
  use wr = new StreamWriter(ctx.Response.OutputStream)
  wr.Write("<h1>Request Failed</h1>")
  wr.Write("<p>" + e.Message + "</p>")
  ctx.Response.Close()

// Handle exception asynchronously - generate page with message
let asyncHandleError (ctx:HttpListenerContext) (e:exn) = async {
  use wr = new StreamWriter(ctx.Response.OutputStream)
  wr.Write("<h1>Request Failed</h1>")
  wr.Write("<p>" + e.Message + "</p>")
  ctx.Response.Close() }
// [/snippet]

module Synchronous = 
  
  // [snippet:Version #1: Synchronous proxy server]
  /// Handle request on a dedicated thread - This is not 
  /// scalable, because thread may be blocked for long time
  let handleRequest (ctx:HttpListenerContext) =
    let wc = new WebClient()
    try
      let data = wc.DownloadData(getProxyUrl(ctx))
      ctx.Response.OutputStream.Write(data, 0, data.Length)
      ctx.Response.Close()
    with e ->
      handleError ctx e
  
  // Start synchronous HTTP proxy 
  let token = HttpListener.StartSynchronous("http://localhost:8080/", handleRequest)
  token.Cancel()
  // [/snippet]

module EventBased = 

  // [snippet:Version #2: Event-based proxy server]  
  module Wrappers =
    (*[omit:Wrappers that provide event-based network API]*)
    type Stream(stream:IO.Stream) = 
      /// Writes data to stream. Calls 'success' callback when 
      /// completed or 'error' callback when error occurs.
      member x.Write(buffer, offset, count, success, error) =
        stream.BeginWrite(buffer, offset, count, (fun ar ->
          try success(stream.EndRead(ar))
          with e -> error(e)), null) |> ignore

    type HttpListenerResponse(rsp:System.Net.HttpListenerResponse) =
      member x.OutputStream = Stream(rsp.OutputStream)
      member x.Close() = rsp.Close()

    type HttpListenerContext(ctx:System.Net.HttpListenerContext) =
      member x.Response = HttpListenerResponse(ctx.Response)
      member x.Request = ctx.Request
      member x.Context = ctx

    type WebClient() = 
      let wc = new System.Net.WebClient()
      /// Downloads data and calls the 'success' callback when the
      /// download completes or 'error' callback if error occurs.
      member x.DownloadData(uri:Uri, success, error) =
        wc.DownloadDataCompleted.Add(fun res ->
          if res.Error <> null then error res.Error
          else success res.Result)
        wc.DownloadDataAsync(uri)(*[/omit]*)

  open Wrappers

  /// Handles request using callbacks - This scales well, but the
  /// code is difficult to write, because we need to use callbacks
  /// (This is similar to the style used by Node.js)
  let handleRequestCallback (ctx:HttpListenerContext) =
    let wc = new WebClient()
    wc.DownloadData
      ( getProxyUrl(ctx.Context), 
        success = (fun data ->
          ctx.Response.OutputStream.Write
            ( data, 0, data.Length,
              success = (fun _ -> ctx.Response.Close()),
              error = handleError ctx.Context )),
        error = handleError ctx.Context )

  // Start HTTP proxy that handles requests using callbacks 
  let token = HttpListener.StartSynchronous("http://localhost:8080/", fun ctx ->
    handleRequestCallback(HttpListenerContext(ctx)))
  token.Cancel()
  // [/snippet]

module Async = 

  // [snippet:Version #3: Asynchronous proxy server]

  /// Handles request using asynchronous workflows - We get efficient & scalable
  /// code by wrapping synchronous solution in 'async' block and changing
  /// synchronous primitives to asynchronous (e.g. AsyncWrite)  
  let asyncHandleRequest (ctx:HttpListenerContext) = async {
    let wc = new WebClient()
    try
      let! data = wc.AsyncDownloadData(getProxyUrl(ctx))
      do! ctx.Response.OutputStream.AsyncWrite(data) 
    with e ->
      do! asyncHandleError ctx e }

  // Start HTTP proxy that handles requests asynchronously
  let token = HttpListener.Start("http://localhost:8080/", asyncHandleRequest)
  token.Cancel()
  // [/snippet]