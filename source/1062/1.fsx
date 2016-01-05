open System
open System.Threading

type ComputationState<'a> =
    | NotStarted
    | Started
    | Completed of 'a

/// Allows to expose a F# async value in a C#-friendly API with the
/// semantics of Lazy<> (compute on demand and guarantee only one computation)
type LazyAsync<'a>(asyncValue:Async<'a>, ?onValueComputed) = 

    let state = ref NotStarted
    let completed = Event<_>()
    do match onValueComputed with
       | Some onValueComputed -> completed.Publish.Add onValueComputed 
       | None -> ()

    /// callBack will be called on the same thread that called GetValueAsync
    member __.GetValueAsync(callBack:Action<'a>) = 

        let synchronizationContext = SynchronizationContext.Current

        let doInOriginalThread f arg = 
            synchronizationContext.Post ((fun _ -> f arg), null)

        let continuation = 
            lock state (fun () -> 
                match !state with
                | Completed value ->
                    let continuation() =
                        doInOriginalThread callBack.Invoke value
                    Some continuation
                | Started ->
                    completed.Publish.Add (doInOriginalThread callBack.Invoke)
                    None
                | NotStarted ->
                    state := Started
                    completed.Publish.Add (doInOriginalThread callBack.Invoke)
                    let continuation() = 
                        async {                                    
                                let! value = asyncValue
                                lock state (fun () -> state := Completed value)
                                completed.Trigger value
                        } |> Async.Start
                    Some continuation)
        
        match continuation with
        | Some continuation -> continuation()
        | _ -> ()