// Simulating supercompilation using quotations

(*[omit:(Linking to FSharp Powerpack)]*)
#r "FSharp.Powerpack.Linq"
(*[/omit]*)


// Defining short synonim for quotation evaluation
let qeval = Microsoft.FSharp.Linq.QuotationEvaluator.Evaluate
