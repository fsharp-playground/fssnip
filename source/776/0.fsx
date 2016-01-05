#r "FSharp.Powerpack.dll"
#r "FSharp.Powerpack.Linq.dll"

open System.Collections.Generic

let create<'T> x = Microsoft.FSharp.Collections.Tagged.Set<'T,IComparer<'T>>.Create(x)

let qeval = Microsoft.FSharp.Linq.QuotationEvaluator.Evaluate