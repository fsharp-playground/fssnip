open RDotNet
open RProvider
open RProvider.``base``
open RProvider.stats

let (?) (x:SymbolicExpression) name = 
  let nameLookup = 
    x.GetAttribute("names").AsList() 
    |> Seq.mapi (fun i n -> n.GetValue<string>(), i)  |> dict
  x.AsList().[nameLookup.[name]]

// call R function
let x = 0.1
let f = R.eval(R.parse(text="function(a){(a - 2)^2}"))
let minX = R.optim(x, f)

// access results
minX?counts
minX?value