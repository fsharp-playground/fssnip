// Simulating supercompilation using quotations

(*[omit:(Linking to FSharp Powerpack)]*)
#r "FSharp.Powerpack.Linq"
(*[/omit]*)

let qeval = Microsoft.FSharp.Linq.QuotationEvaluator.Evaluate