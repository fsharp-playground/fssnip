open System

let private internalPreserveStackTrace = typeof<Exception>.GetMethod("InternalPreserveStackTrace", System.Reflection.BindingFlags.Instance ||| System.Reflection.BindingFlags.NonPublic)
let private preserveStackTrace (e:Exception) = internalPreserveStackTrace.Invoke(e, null) |> ignore; e

/// Raises the exception given after attempting to run each of the handlers in sequence.
/// If any exceptions are encountered during execution of the handlers, they will
/// be aggregated with the original exception. Otherwise the exception will be
/// raised in its original state, i.e. with preserved stack trace
let raiseAfterHandling (handlers:#seq<unit -> unit>) (e:Exception) =
    match handlers |> Seq.choose (fun handler -> try handler(); None with e -> Some e) |> Seq.toList with
    | [] -> preserveStackTrace e |> raise
    | list -> AggregateException(e :: list) |> raise
