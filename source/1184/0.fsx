open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Reflection
open System.Threading

/// There is no common superclass of all IObservable<T>.
type IObservable = obj

/// Raised when an observable has not yet published a value.
type DeferredValueException() =
    inherit Exception()

/// Raised when an observable's value is accessed after the observable publishes an error.
type ObservableException(message : string, innerException : Exception) =
    inherit Exception(message, innerException)

/// The last value of an observable, or an error, or neither.
type IObservableValue<'a> =
    abstract HasValue : bool
    abstract Value : 'a
    abstract Exception : Exception

/// Implementation of IObservable<T> and IObservableValue<T>.
type Observable<'a>() =
    let syncRoot = obj()
    let mutable subs : IObserver<'a> list = [ ]
    let mutable lastValue : Choice<Exception, 'a> option = None

    let rec removeFirst value =
        function
        | x :: xs when x = value -> xs
        | _ :: xs -> removeFirst value xs
        | [ ] -> [ ]

    member t.Publish(value) =
        let subs =
            lock syncRoot <| fun () ->
                lastValue <- Some (Choice2Of2 value)
                subs

        for sub in subs do sub.OnNext(value)

    member t.PublishError(ex) =
        let subs =
            lock syncRoot <| fun () ->
                lastValue <- Some (Choice1Of2 ex)
                subs

        for sub in subs do sub.OnError(ex)

    interface IObservable<'a> with
        member t.Subscribe(sub) =
            lock syncRoot <| fun () ->
                subs <- sub :: subs

            { new IDisposable with
                member x.Dispose() =
                    lock syncRoot <| fun () ->
                        subs <- removeFirst sub subs
            }

    interface IObservableValue<'a> with
        member t.HasValue = Option.isSome lastValue

        member t.Value =
            match lastValue with
            | Some (Choice1Of2 ex) -> raise (ObservableException(ex.Message, ex))
            | Some (Choice2Of2 value) -> value
            | None -> Unchecked.defaultof<_>

        member t.Exception =
            match lastValue with
            | Some (Choice1Of2 ex) -> ex
            | _ -> null

/// Implementation of IObserver<T>, in terms of a pair of F# functions.
type Observer<'a>(next : 'a -> unit, error : Exception -> unit) =
    interface IObserver<'a> with
        member t.OnNext(value) =
            next value
            
        member t.OnError(ex) =
            error ex
            
        member t.OnCompleted() =
            ()

module private Impl =

    type Type with
        member t.CheckedGetMethod(name : string, bindingFlags : BindingFlags) =
            match t.GetMethod(name, bindingFlags) with
            | null -> failwithf "No method '%s' on %O" name t
            | mi -> mi

    let memoize (fn : 'a -> 'b) : 'a -> 'b =
        let dict = Dictionary()
        fun a ->
            let found, b = lock dict <| fun () -> dict.TryGetValue(a)
            if found then
                b
            else
                let b = fn a
                lock dict <| fun () -> dict.[a] <- b
                b

    type IInvoker =
        abstract Invoke<'a> : unit -> obj

    /// There is no common superclass of all IObservable<T>. Therefore if we have an observable,
    /// and we don't know what 'T' is, we have to use reflection to call the Subscribe method.
    let subscribeAny : Type -> (IObservable * (obj -> unit) * (Exception -> unit)) -> IDisposable =
        let md = typeof<IInvoker>.CheckedGetMethod("Invoke", BindingFlags.Public ||| BindingFlags.Instance)
        memoize <| fun typ ->
            let picker (iface : Type) =
                if iface.IsGenericType && iface.GetGenericTypeDefinition() = typedefof<IObservable<_>> then
                    Some (iface.GetGenericArguments().[0])
                else
                    None

            let valueType =
                match Array.tryPick picker (typ.GetInterfaces()) with
                | Some typ -> typ
                | None -> failwithf "%O does not implement IObservable<T>" typ

            let mi = md.MakeGenericMethod([| valueType |])

            fun (obs, next, error) ->
                let invoker =
                    { new IInvoker with
                        member t.Invoke<'a>() =
                            (obs :?> IObservable<'a>).Subscribe(Observer(next, error)) :> _
                    }

                unbox (mi.Invoke(invoker, [| |]))

module Observable =

    [<AbstractClass>]
    type internal Monitor() =
        [<ThreadStatic; DefaultValue>]
        static val mutable private current : Monitor option

        static member Current : Monitor =
            match Monitor.current with
            | Some m -> m
            | None -> failwithf "Not inside Observable.Computed"
            
        static member SetCurrent(m : Monitor) : IDisposable =
            let oldCurrent = Monitor.current
            Monitor.current <- Some m
            { new IDisposable with member t.Dispose() = Monitor.current <- oldCurrent }

        abstract GetValue<'a> : obs : IObservable<'a> -> 'a

    let computed<'a> (fn : unit -> 'a) : Observable<'a> =
        let values = ConcurrentDictionary<IObservable, Choice<Exception, obj>>()
        let syncRoot = obj()
        let subs = Dictionary<IObservable, IDisposable>()
        let observable = Observable<'a>()

        // Call the function and, using the Monitor class, track the observables that it accesses.
        //  Subscribe to any new ones, and unsubscribe from any stale ones. Halt evaluation if the
        //  function accesses an observable for which we haven't seen a value (but still keep our
        //  subscriptions, so we get notified when a value does eventually arrive).
        // Our IObservableValue<T> interface reduces the need to throw a DeferredValueException,
        //  by letting us access the last published value without having a subscription.
        let rec refresh () =
            let obses = HashSet<IObservable>()
            let m =
                { new Monitor() with
                    member t.GetValue<'b>(obs) =
                        let obs = obs :> IObservable
                        ignore (obses.Add(obs))

                        match obs with
                        | :? IObservableValue<'b> as lv ->
                            if lv.HasValue then
                                lv.Value
                            else
                                raise (DeferredValueException())

                        | _ ->
                            match values.TryGetValue(obs) with
                            | true, Choice1Of2 ex -> raise (ObservableException(ex.Message, ex))
                            | true, Choice2Of2 value -> unbox value
                            | false, _ -> raise (DeferredValueException())
                }

            let value =
                using (Monitor.SetCurrent(m)) <| fun _ ->
                    try
                        Some (Choice2Of2 (fn ()))
                    with
                    | :? DeferredValueException -> None
                    | :? ObservableException as ex -> Some (Choice1Of2 ex.InnerException)
                    | ex -> Some (Choice1Of2 ex)

            lock syncRoot <| fun () ->
                for pair in Array.ofSeq subs do
                    if not (obses.Contains(pair.Key)) then
                        pair.Value.Dispose()
                        ignore (subs.Remove(pair.Key))

                for obs in obses do
                    if not (subs.ContainsKey(obs)) then
                        let on value =
                            values.[obs] <- value
                            refresh ()

                        subs.[obs] <- Impl.subscribeAny (obs.GetType()) (obs, Choice2Of2 >> on, Choice1Of2 >> on)

            match value with
            | Some (Choice1Of2 ex) -> observable.PublishError(ex)
            | Some (Choice2Of2 value) -> observable.Publish(value)
            | None -> ()

        refresh ()
        observable
        

[<AutoOpen>]
module ObservableExtensions =

    type IObservable<'a> with
        member t.Value : 'a =
            // The key to making the whole thing work. (If we bypass this extension property,
            //  and access the observable's last value directly, we don't get chance to
            //  subscribe to the observable.)
            Observable.Monitor.Current.GetValue(t)

        member t.Subscribe(next : 'a -> unit, error : Exception -> unit) : IDisposable =
            // A helper method for the demo below. F# provides Subscribe('a -> unit),
            //  but we want to handle errors too.
            t.Subscribe(Observer(next, error))

[<EntryPoint>]
let main args =
    let obs1 = Observable()
    use d1 = obs1.Subscribe(printfn "Got obs1: %d", fun ex -> printfn "Error obs1: %s" ex.Message)
    let obs2 = Observable()
    use d2 = obs2.Subscribe (printfn "Got obs2: %d", fun ex -> printfn "Error obs2: %s" ex.Message)
    let c =
        Observable.computed <| fun () ->
            if obs2.Value < 10 then
                failwithf "%d is too low" obs2.Value
            else
                obs1.Value + obs2.Value

    use dc = c.Subscribe (printfn "Got computed: %d", fun ex -> printfn "Error computed: %s" ex.Message)
    obs1.Publish(10)

    let r = Random()
    while true do
        obs2.Publish(r.Next(0, 20))
        Thread.Sleep(1000)

    0
