open System
open System.Collections.Generic

type Dictionary<'TKey, 'TValue, 'TComparer when 'TKey : equality and 'TComparer :> IEqualityComparer<'TKey>>(comparer: 'TComparer) =
    inherit Dictionary<'TKey, 'TValue>(comparer)

type DelegatingEqualityComparer<'a>(comparer: IEqualityComparer<'a>) = 
    interface IEqualityComparer<'a> with
        member x.Equals(a,b) = comparer.Equals(a,b)
        member x.GetHashCode a = comparer.GetHashCode a

type StringComparerInvariantCultureIgnoreCase() =
    inherit DelegatingEqualityComparer<string>(StringComparer.InvariantCultureIgnoreCase)

type StringComparerInvariantCulture() =
    inherit DelegatingEqualityComparer<string>(StringComparer.InvariantCulture)

let doSomething (data: Dictionary<string, _, StringComparerInvariantCultureIgnoreCase>) =
    printfn "%A" data.["one"]
    printfn "%A" data.["two"]

let data = Dictionary<_,int,_>(StringComparerInvariantCulture())
data.["One"] <- 1
data.["Two"] <- 2
doSomething data // doesn't compile, as the comparers are different!
let data2 = Dictionary<_,int,_>(StringComparerInvariantCultureIgnoreCase())
for KeyValue(k,v) in data do data2.Add(k,v)
doSomething data2 // works as expected
