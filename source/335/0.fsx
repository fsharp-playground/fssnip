open System
  
/// Represents an object that is both 
/// an observable sequence as well as 
/// an observer
type ReplaySubject<'T> (bufferSize:int) =
    let buffer = Array.zeroCreate bufferSize
    let mutable index, total = 0, 0
    let mutable stopped = false
    let observers = System.Collections.Generic.List<IObserver<'T>>()
    let iter f = observers |> Seq.iter f          
    let onCompleted () =
        if not stopped then
            stopped <- true
            iter (fun observer -> observer.OnCompleted())
    let onError ex () =
        if not stopped then
            stopped <- true
            iter (fun observer -> observer.OnError(ex))          
    let next value () =
        if not stopped then
            if bufferSize > 0 then
                buffer.[index] <- value
                index <- (index + 1) % bufferSize
                total <- min (total + 1) bufferSize
            iter (fun observer -> observer.OnNext value)
    let add observer () =
        observers.Add observer
        let start = if total = bufferSize then index else 0
        for i = 0 to total-1 do 
            buffer.[(start + i)%bufferSize] |> observer.OnNext
    let remove observer () =
        observers.Remove observer |> ignore
    let sync = obj()
    member x.Next value = lock sync <| next value
    member x.Error ex = lock sync <| onError ex
    member x.Completed () = lock sync <| onCompleted
    interface IObserver<'T> with
        member x.OnCompleted() = x.Completed()
        member x.OnError ex = x.Error ex
        member x.OnNext value = x.Next value    
    interface IObservable<'T> with
        member this.Subscribe(observer:IObserver<'T>) =
            lock sync <| add observer
            { new IDisposable with
                member this.Dispose() =
                    lock sync <| remove observer
            }
and Subject<'T> () =
    inherit ReplaySubject<'T> (0)


do  let s = ReplaySubject(10)
    use d = s.Subscribe(fun x -> sprintf "%d" x |> Console.WriteLine)
    [1..16] |> Seq.iter s.Next
    use d' = s.Subscribe(fun x -> sprintf "'%d" x |> Console.WriteLine)
    ()