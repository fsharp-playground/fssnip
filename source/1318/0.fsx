open System
open System.Collections
open System.Runtime.Serialization

type ExceptionContext =
    {
        Data : IDictionary
        HelpUrl : string
        StackTrace : string
        RemoteStackTrace : string
        RemoteStackIndex : int
        ExceptionMethod : string
        HResult : int
        Source : string
        WatsonBuckets : byte []
    }
with
    static member OfException(e : exn) =
        let sI = new SerializationInfo(typeof<exn>, FormatterConverter())
        let sc = new StreamingContext()
        e.GetObjectData(sI,sc)
        {
            Data = sI.GetValue("Data", typeof<IDictionary>) :?> IDictionary
            HelpUrl = sI.GetString("HelpURL")
            StackTrace = sI.GetString("StackTraceString")
            RemoteStackTrace = sI.GetString("RemoteStackTraceString")
            RemoteStackIndex = sI.GetInt32("RemoteStackIndex")
            ExceptionMethod = sI.GetString("ExceptionMethod")
            HResult = sI.GetInt32("HResult")
            Source = sI.GetString("Source")
            WatsonBuckets = sI.GetValue("WatsonBuckets", typeof<byte []>) :?> byte []
        }

    member internal c.WriteContextData(sI : SerializationInfo) =
        sI.AddValue("Data", c.Data, typeof<IDictionary>)
        sI.AddValue("HelpURL", c.HelpUrl)
        sI.AddValue("StackTraceString", c.StackTrace)
        sI.AddValue("RemoteStackTraceString", c.RemoteStackTrace)
        sI.AddValue("RemoteStackIndex", c.RemoteStackIndex)
        sI.AddValue("ExceptionMethod", c.ExceptionMethod)
        sI.AddValue("HResult", c.HResult)
        sI.AddValue("Source", c.Source)
        sI.AddValue("WatsonBuckets", c.WatsonBuckets, typeof<byte []>)

type ContextualException =
    inherit Exception

    new(message : string, ?inner : exn, ?context : ExceptionContext) =
        match inner, context with
        | None, None -> { inherit Exception(message) }
        | Some e, None -> { inherit Exception(message, e) }
        | _, Some context ->
            let sI = new SerializationInfo(typeof<exn>, FormatterConverter())
            let sc = new StreamingContext()
            sI.AddValue("ClassName", "System.Exception")
            sI.AddValue("Message", message)
            sI.AddValue("InnerException", defaultArg inner null, typeof<Exception>)
            context.WriteContextData(sI)
            { inherit Exception(sI, sc) }

    member e.GetExceptionContext() = ExceptionContext.OfException e


// example

type Test(msg : string, ?inner, ?ctx) =
    inherit ContextualException(msg, ?inner = inner, ?context = ctx)

// throws a deep exception
let rec dig n =
    if n = 0 then raise <| Test("foo")
    else
        1 + dig (n-1)

// catch
let e = try dig 40 |> ignore ; failwith "" with :? Test as e -> e

// evaluate
let ctx = e.GetExceptionContext()

// patch
let e' = Test("bar", inner = System.Exception("nested"), ctx = ctx)

// verify
e'.ToString()