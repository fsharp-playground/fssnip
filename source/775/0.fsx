#r "FSharp.Powerpack.dll"

open System.Collections.Generic

let create<'T> x = Microsoft.FSharp.Collections.Tagged.Set<'T,IComparer<'T>>.Create(x)