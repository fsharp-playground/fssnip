module Sayuri.Net.SocketExtensions
open System
open System.Net
open System.Net.Sockets

#if NET4
type private ConcurrentBag<'T> = System.Collections.Concurrent.ConcurrentBag<'T>
#else
type private ConcurrentBag<'T>() =
    let bag = System.Collections.Generic.Stack<'T>()
    member this.TryTake() =
        lock bag (fun () -> if 0 < bag.Count then true, bag.Pop() else false, Unchecked.defaultof<_>)
    member this.Add(item) =
        lock bag (fun () -> bag.Push item)
#endif

let inline private checkNonNull name arg =
    match box arg with null -> nullArg name | _ -> ()


let private pool = ConcurrentBag()

let private invoke methodAsync prepare result = async {
    let e = match pool.TryTake() with
            | true, e  -> e
            | false, _ -> new SocketAsyncEventArgs()
    try
        prepare e
        return! Async.FromContinuations(fun (cont, econt, _) ->
            let called = ref 0
            let completed (e : SocketAsyncEventArgs) =
                assert(System.Threading.Interlocked.Increment called = 1)
                (e.UserToken :?> IDisposable).Dispose()
#if NET4
                if e.ConnectByNameError <> null then econt e.ConnectByNameError else
#endif
                if e.SocketError <> SocketError.Success then new SocketException(int e.SocketError) |> econt else
                result e |> cont
            e.UserToken <- e.Completed.Subscribe completed
            if methodAsync e |> not then completed e
        )
    finally
        e.AcceptSocket <- null
        e.BufferList <- null
        e.RemoteEndPoint <- null
        e.SocketFlags <- SocketFlags.None
        e.UserToken <- null
        e.SetBuffer(null, 0, 0)
        pool.Add(e)
    }

let private setBuffer buffer offset count (e : SocketAsyncEventArgs) =
    let offset = defaultArg offset 0
    let count = defaultArg count (Array.length buffer - offset)
    e.SetBuffer(buffer, offset, count)

type Socket with 
    member this.AsyncAccept() =
        invoke this.AcceptAsync
            ignore
            (fun e -> e.AcceptSocket)
    member this.AsyncAccept(buffer, ?offset, ?count) =
        invoke this.AcceptAsync
            (fun e -> setBuffer buffer offset count e
                      assert ((this.LocalEndPoint.Serialize().Size + 16) * 2 < e.Count))    // test buffer size.
            (fun e -> e.AcceptSocket, e.BytesTransferred)
    member this.AsyncAccept(acceptSocket) =
        checkNonNull "acceptSocket" acceptSocket
        invoke this.AcceptAsync
            (fun e -> e.AcceptSocket <- acceptSocket)
            ignore
    member this.AsyncAccept(acceptSocket, buffer, ?offset, ?count) =
        checkNonNull "acceptSocket" acceptSocket
        checkNonNull "buffer" buffer
        invoke this.AcceptAsync
            (fun e -> setBuffer buffer offset count e
                      assert ((this.LocalEndPoint.Serialize().Size + 16) * 2 < e.Count)     // test buffer size.
                      e.AcceptSocket <- acceptSocket)
            (fun e -> e.BytesTransferred)
    member this.AsyncConnect(remoteEndPoint) =
        checkNonNull "remoteEndPoint" remoteEndPoint
        invoke this.ConnectAsync
            (fun e -> e.RemoteEndPoint <- remoteEndPoint)
            ignore
    member this.AsyncConnect(remoteEndPoint, buffer, ?offset, ?count) =
        checkNonNull "remoteEndPoint" remoteEndPoint
        checkNonNull "buffer" buffer
        invoke this.ConnectAsync
            (fun e -> setBuffer buffer offset count e
                      e.RemoteEndPoint <- remoteEndPoint)
            (fun e -> e.BytesTransferred)
    member this.AsyncConnect(host, port) =
        checkNonNull "host" host
        if port < IPEndPoint.MinPort || IPEndPoint.MaxPort < port then ArgumentOutOfRangeException "port" |> raise
#if NET4
        invoke this.ConnectAsync
            (fun e -> e.RemoteEndPoint <- DnsEndPoint(host, port))
            ignore
#else
        Async.FromBeginEnd<string, _, _>(host, port, this.BeginConnect, this.EndConnect)
#endif
    member this.AsyncDisconnect(reuseSocket) =
        invoke this.DisconnectAsync
            (fun e -> e.DisconnectReuseSocket <- reuseSocket)
            ignore
    member this.AsyncReceive(buffer, ?offset, ?count, ?socketFlags) =
        checkNonNull "buffer" buffer
        invoke this.ReceiveAsync
            (fun e -> setBuffer buffer offset count e
                      e.SocketFlags <- defaultArg socketFlags SocketFlags.None)
            (fun e -> e.BytesTransferred)
    member this.AsyncReceive(bufferList, ?socketFlags) =
        checkNonNull "bufferList" bufferList
        invoke this.ReceiveAsync
            (fun e -> e.BufferList <- bufferList
                      e.SocketFlags <- defaultArg socketFlags SocketFlags.None)
            (fun e -> e.BytesTransferred)
    member this.AsyncSend(buffer, ?offset, ?count, ?socketFlags) =
        checkNonNull "buffer" buffer
        invoke this.SendAsync
            (fun e -> setBuffer buffer offset count e
                      e.SocketFlags <- defaultArg socketFlags SocketFlags.None)
            (fun e -> e.BytesTransferred)
    member this.AsyncSend(bufferList, ?socketFlags) =
        checkNonNull "bufferList" bufferList
        invoke this.SendAsync
            (fun e -> e.BufferList <- bufferList
                      e.SocketFlags <- defaultArg socketFlags SocketFlags.None)
            (fun e -> e.BytesTransferred)
