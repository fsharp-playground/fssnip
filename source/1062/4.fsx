module FSharp.Control

open System
open System.Threading

type AsyncState<'a> =
    | NotStarted of Async<'a>
    | Started
    | Completed of 'a

/// Allows to expose a F# async value in a C#-friendly API with the
/// semantics of Lazy<> (compute on demand and guarantee only one computation)
type LazyAsync<'a>(state:AsyncState<'a>) = 

    let state = ref state
    let completed = Event<_>()
    let icompleted = completed.Publish

    member private __.doWhenCompleted f startIfNotRunning =   
        let continuation =      
            lock state <| fun () -> 
                match !state with
                | Completed value ->
                    Some <| fun () -> f value
                | Started ->
                    icompleted.Add f
                    None
                | NotStarted asyncValue ->
                    icompleted.Add f
                    if startIfNotRunning then
                        state := Started
                        Some <| fun () ->
                            async {
                                try
                                    let! value = asyncValue
                                    lock state <| fun () -> state := Completed value
                                    completed.Trigger value
                                with _ -> ()
                            } |> Async.Start
                    else
                        None
        match continuation with
        | Some continuation -> continuation()
        | _ -> ()

    /// Will start calculation if not started
    /// callBack will be called on the same thread that called GetValueAsync
    member x.GetValueAsync(callBack:Action<_>) = 

        let synchronizationContext = SynchronizationContext.Current

        let doInOriginalThread f arg = 
            if synchronizationContext <> null then
                synchronizationContext.Post ((fun _ -> f arg), null)
            else
                f arg

        x.doWhenCompleted (doInOriginalThread callBack.Invoke) true

    member x.Subscribe f = 
        x.doWhenCompleted f false
        x

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module LazyAsync =

    let fromAsync asyncValue = 
        LazyAsync(NotStarted asyncValue)

    let fromValue value =
        LazyAsync(Completed value)

    let map f (x:LazyAsync<'a>) =
        async { 
            let ev = Event<_>()
            x.GetValueAsync(fun value -> ev.Trigger value)
            let! value = Async.AwaitEvent(ev.Publish)
            return f value 
        } |> fromAsync

    let subscribe f (x:LazyAsync<'a>) =
        x.Subscribe f
