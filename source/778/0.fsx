// Simulating supercompilation using quotations

(*[omit:(Linking to FSharp Powerpack)]*)
#r "FSharp.Powerpack.Linq"
(*[/omit]*)

open Microsoft.FSharp.Linq

let foo = QuotationEvaluator.Evaluate