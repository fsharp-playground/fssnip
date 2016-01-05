// [snippet:Quotation transformation]
open Microsoft.FSharp.Quotations

/// Traverse an entire quotation and use the provided function
/// to transform some parts of the quotation. If the function 'f'
/// returns 'Some' for some sub-quotation then we replace that
/// part of the quotation. The function then recursively processes
/// the quotation tree.
let rec traverseQuotation f q = 
  let q = defaultArg (f q) q
  match q with
  | ExprShape.ShapeCombination(a, args) -> 
      let nargs = args |> List.map (traverseQuotation f)
      ExprShape.RebuildShapeCombination(a, nargs)
  | ExprShape.ShapeLambda(v, body)  -> 
      Expr.Lambda(v, traverseQuotation f body)
  | ExprShape.ShapeVar(v) ->
      Expr.Var(v)

// Sample quotation (written explicitly using <@ .. @>)
let quot = 
 <@ let a = 10 
    let b = 32 / a
    a / b @>
// [/snippet]

// [snippet:Example: Finding constants]
/// Find all constants in the quotation and print them...  
let findConstants quot =
  quot |> traverseQuotation (fun q -> 
    match q with 
    | Patterns.Value(v, typ) -> printfn "Constant: %A" v
    | _ -> ()
    None ) 
  |> ignore

findConstants quot  
// [/snippet]

// [snippet:Example: Multiply constants by two]
/// Multiply all integer constants by two and compile the 
/// returned quotation & evaluate it
let quotTwoTimes quot = 
  quot |> traverseQuotation (fun q -> 
    match q with 
    | Patterns.Value(v, typ) when typ = typeof<int> ->
        Some(Expr.Value((unbox v) * 2))
    | _ -> None )

// Compile & run modified quotation
#r "FSharp.PowerPack.Linq.dll"
open Microsoft.FSharp.Linq.QuotationEvaluation
    
let quotTwoTimesTyped = Expr.Cast<int>(quotTwoTimes quot)    
quotTwoTimesTyped.Eval()
// [/snippet]