
open System
open System.Threading
open System.Collections.Generic
open System.Collections.Concurrent


type IResource<'T> = abstract ThreadLocalValue : 'T

/// Thread-local portable dependency injection
type ThreadContext private () =

    static let factoryCount = ref 0
    static let resourceFactories = new ConcurrentDictionary<int, unit -> obj> () :> IDictionary<_,_>
    static let threadLocalState = new ThreadLocal<ThreadContext option ref>(fun () -> ref None)

    let resourceContainer = new ConcurrentDictionary<int, obj> ()

    member private __.GetResource<'T> (id : int) =
        let ok, value = resourceContainer.TryGetValue id
        if ok then value :?> 'T
        else
            // resource not installed in context, perform installation now
            let ok, factory = resourceFactories.TryGetValue id
            if ok then
                resourceContainer.AddOrUpdate(id, (fun _ -> factory ()), (fun _ value -> value)) :?> 'T
            else
                failwithf "ThreadContext: no factory for resource of type '%O' has been installed." typeof<'T>

    static member private GetResource<'T> id =
        match threadLocalState.Value.Value with
        | None -> failwith "ThreadContext: no context is installed on current thread."
        | Some ctx -> ctx.GetResource<'T> id

    /// installs given context to the current thread.
    member ctx.InstallContextToCurrentThread () : IDisposable =
        let state = threadLocalState.Value
        match state.Value with
        | Some _ -> invalidOp "ThreadContext: a context is already installed on this thread."
        | None ->
            state := Some ctx
            let isDisposed = ref 0
            { 
                new IDisposable with 
                    member __.Dispose () =
                        if Interlocked.CompareExchange(isDisposed, 1, 0) = 0 then
                            state := None
            }

    /// defines a new global resource
    static member InstallResourceFactory(f : unit -> 'T) =
        let id = Interlocked.Increment(factoryCount)
        resourceFactories.Add(id, fun () -> f () :> obj)
        { new IResource<'T> with member __.ThreadLocalValue = ThreadContext.GetResource<'T>(id) }

    static member Create () = new ThreadContext ()


// example

/// install a global resource
let globalMutableString = ThreadContext.InstallResourceFactory(fun () -> ref "")

// initialize a collection of contexts
let ctxs = Array.init 10 (fun _ -> ThreadContext.Create())

async {
    let store (value : int) (ctx : ThreadContext) = async {
        // 'use' binding keeps context installed only within lexical scope
        use uninstaller = ctx.InstallContextToCurrentThread()
        globalMutableString.ThreadLocalValue := string value
    }

    do! ctxs |> Array.mapi store |> Async.Parallel |> Async.Ignore
    
    // uncomment to cause exception
    //printfn "%s" globalMutableString.ThreadLocalValue.Value

    let read (ctx : ThreadContext) = async {
        use uninstaller = ctx.InstallContextToCurrentThread()
        return globalMutableString.ThreadLocalValue.Value
    }

    // output should maintain order of indices
    return! ctxs |> Array.map read |> Async.Parallel

} |> Async.RunSynchronously