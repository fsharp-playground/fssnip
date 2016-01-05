#r "FSharp.Powerpack.dll"

open System.Collections.Generic
open Microsoft.FSharp.Collections

module TagSet =

    let create (f : 'T -> 'D) (input : #seq<'T>) =
        let comparer =
            {
                new IComparer<'T> with
                    member __.Compare(x,y) = compare (f x) (f y)
            }

        Tagged.Set<_,_>.Create(comparer, input)

// example 1
let modSet = TagSet.create (fun x -> x % 5) <| seq { 1 .. 1000 }

modSet.ToList()

// example 2
open System
open System.Reflection

let typeSet = TagSet.create (fun (t: Type) -> t.GUID) <| Assembly.GetExecutingAssembly().GetTypes()

typeSet.ToList()