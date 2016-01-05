// Simulating supercompilation using quotations

(*[omit:(Linking to FSharp Powerpack)]*)
#r "FSharp.Powerpack.Linq"
(*[/omit]*)

// Raising x to the power of n
// Returns quotation expression that calculates the result
// metapower 5 x returns <@ x*x*x*x*x @>
let rec metapower n x =
  if n=0 then <@ 1 @>
  else (fun z -> <@ %x * %z @>) (metapower(n-1) x)

// Defining short synonim for quotation evaluation
let qeval = Microsoft.FSharp.Linq.QuotationEvaluator.Evaluate

// Raising arbitrary number to the power of 5
// Gets the quotation with expression x*x*x*x*x and evaluates it
let pow5 x = metapower 5 <@ x @> |> qeval

printfn "%d" (pow5 10)
