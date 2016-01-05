// Rx 1.0:
//#r "System.CoreEx" //for interactive and scripts
//#r "System.Reactive.dll" 
//open System
//open System.Collections.Generic

//Rx 2.1:
#r "System.ComponentModel.Composition.dll"
#r "../packages/Rx-Interfaces.2.1.30214.0/lib/Net45/System.Reactive.Interfaces.dll"
#r "../packages/Rx-Core.2.1.30214.0/lib/Net45/System.Reactive.Core.dll"
#r "../packages/Rx-Linq.2.1.30214.0/lib/Net45/System.Reactive.Linq.dll"
open System.Reactive.Subjects

// ---

let observable = new Subject<int>()

let mySubscribe =
    let interested = observable |> Observable.filter (fun x -> x%2=0)
    interested.Subscribe(fun i -> Console.WriteLine("Hello " + i.ToString()))

let myYields =
    observable.OnNext(1)
    observable.OnNext(2)
    observable.OnNext(3)
    observable.OnNext(4)


