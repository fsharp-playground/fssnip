open System.Linq

// Extend the standard QueryBuilder type with an additional 
// custom operation (that must be expressed in terms of other
// query operations) and marked with ReflectedDefinition
type Linq.QueryBuilder with
  [<ReflectedDefinition; CustomOperation("exactlyOneOrNone")>]
  member __.ExactlyOneOrNone (source : Linq.QuerySource<'T, 'U>) : 'T option =
    query.ExactlyOneOrDefault(query.Select(source, fun x -> Some x))

[<AutoOpen>]
module QueryExtensions = 
  open Microsoft.FSharp.Quotations

  /// Traverse a quotation and replace expressions according to 'f'
  /// (see also http://fssnip.net/1i)
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

  /// Store the original query.Run operation
  let oldRun (e:Expr<'T>) = query.Run(e)

  /// Add a new 'Run' method that first replaces 'exactlyOneOrNone' 
  /// (and other extensions) with their definition and then runs
  /// the new quotation using previous 'oldRun' method
  type Linq.QueryBuilder with
    [<CompiledName("RunQueryAsValue")>]
    member this.Run (q: Microsoft.FSharp.Quotations.Expr<'T>) : 'T = 
      let q : Expr<'T> = 
        q |> traverseQuotation (function
          // Detects a call to an (instance) method that has the ReflectedDefinition attribute
          // and replaces it with the body of the method (taken from Query.fs of FSharp.Core.dll)
          | Patterns.Call(Some inst, DerivedPatterns.MethodWithReflectedDefinition(DerivedPatterns.Lambdas(vs, body)), args) -> 
              let args = inst::args
              let tab = 
                List.map2 (fun (vs:Var list) arg -> 
                  match vs, arg with 
                  | [v], arg -> [(v, arg)] | vs, Patterns.NewTuple(args) -> List.zip vs args 
                  | _ -> List.zip vs [arg]) vs args
                |> List.concat
                |> Map.ofSeq
              let body = body.Substitute tab.TryFind 
              Some body
          | _ -> None) |> Expr.Cast
      oldRun(q)

// Example - now we can use 'exactlyOneOrNone'!                                       
let data = List.empty<int>
let value = query { for v in data do
                    select v
                    exactlyOneOrNone }  
printf "%A" value