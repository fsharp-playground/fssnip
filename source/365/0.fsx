open System
open System.ComponentModel
open System.Threading

/// Executes a computation in a background worker and synchronizes on the result return.  The
/// computation is started immediately and calling 'Value' blocks until the result is ready.
type Future<'t>(f : unit -> 't) =
    /// Result of the computation on normal exit.
    let mutable result :'t option = None
    
    /// Result if an exception was thrown.
    let mutable ext : Exception option = None
    
    let syncRoot = new Object()
    
    /// Pulse object used to wait until a result is ready.  ensurePulse() is used so we
    /// don't have to create the object if the result is done before it's needed.
    let mutable pulse : ManualResetEvent = null
    let ensurePulse() = 
        lock syncRoot (fun () ->
            match pulse with 
            | null -> 
                pulse <- new ManualResetEvent(false);
            | _ -> 
                ()
            pulse)
        
    /// WARNING: Call once a lock on syncRoot is already held.  Pulses the wait notifier.  Safe if
    /// called after 'pulse' is created but before WaitOne is called.
    let notifyWaiters() = if pulse <> null then pulse.Set() |> ignore
    
    let work = new BackgroundWorker()

    /// On RunWorkerAsync(), run specified function and store result.  All exceptions must be
    /// trapped.      
    do work.DoWork.Add( fun args ->
                            try 
                                result <- Some( f() )
                            with e ->
                                ext <- Some e
                            lock syncRoot ( fun () -> notifyWaiters()) )

    /// Start immediately / automatically.
    do work.RunWorkerAsync()
    
    /// Returns the value of the computation, blocking if the result isn't ready yet.
    member t.Value =
        // If available, we can return it right away.
        match result with
        | Some x -> x
        | None when ext.IsSome -> raise (Option.get ext)
        | None ->
            let p = ensurePulse()
            
            // Check again in case it changed while we were gettting the wait object.
            match result with
            | Some x -> x
            | None ->
                // Lock-free is ok because if the pulse.Set() method is called between when we
                // checked 'result' and call WaitOne here, WaitOne will return immediately.
                p.WaitOne(1000000000) |> ignore
                match result with
                | Some x -> x
                | None ->
                    if ext.IsSome then raise (Option.get ext)
                    else failwith "Future computation failed."

    /// Returns true if the computation is finished, false if not.
    member t.IsComplete =
        match result with
        | Some x -> true
        | None when Option.isSome ext -> true
        | None -> false
    