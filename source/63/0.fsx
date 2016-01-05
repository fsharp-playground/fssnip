#r "FSharp.PowerPack.Linq.dll"
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.QuotationEvaluation
    
// Create a part of expression using "Expr" calls 
// (this creates arbitrary untyped quotation)
let expr = Expr.Value(13)

// Create a part using quotation syntax 
// (splicing untyped part in using %%)
let expr2 = <@ (fun x -> x * %%expr) @>
    
// Compile the quotation & Run returned function
let f = expr2.Compile()()
f 10
