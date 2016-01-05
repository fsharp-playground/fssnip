//#r "System.CoreEx" //for interactive and scripts
//#r "System.Reactive.dll" 
open System
open System.Collections.Generic

let observable = new Subject<int>()

let mySubscribe =
    let interested = observable |> Observable.filter (fun x -> x%2=0)
    interested.Subscribe(fun i -> Console.WriteLine("Hello " + i.ToString()))

let myYields =
    observable.OnNext(1)
    observable.OnNext(2)
    observable.OnNext(3)
    observable.OnNext(4)

