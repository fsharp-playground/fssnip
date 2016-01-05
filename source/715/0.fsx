open Microsoft.FSharp.Quotations
    
/// The parameter 'vars' is an immutable map that assigns expressions to variables
/// (as we recursively process the tree, we replace all known variables)
let rec expand vars expr = 

  // First recursively process & replace variables
  let expanded = 
    match expr with
    // If the variable has an assignment, then replace it with the expression
    | ExprShape.ShapeVar v when Map.containsKey v vars -> vars.[v]
    // Apply 'expand' recursively on all sub-expressions
    | ExprShape.ShapeVar v -> Expr.Var v
    | Patterns.Call(body, DerivedPatterns.MethodWithReflectedDefinition meth, args) ->
        let this = match body with Some b -> Expr.Application(meth, b) | _ -> meth
        let res = Expr.Applications(this, [ for a in args -> [a]])
        expand vars res
    | ExprShape.ShapeLambda(v, expr) -> 
        Expr.Lambda(v, expand vars expr)
    | ExprShape.ShapeCombination(o, exprs) ->
        ExprShape.RebuildShapeCombination(o, List.map (expand vars) exprs)

  // After expanding, try reducing the expression - we can replace 'let'
  // expressions and applications where the first argument is lambda
  match expanded with
  | Patterns.Application(ExprShape.ShapeLambda(v, body), assign)
  | Patterns.Let(v, assign, body) ->
      expand (Map.add v (expand vars assign) vars) body
  | _ -> expanded


// The following example replaces the function `foo` with its 
// body and then replaces the application, so you end up 
// with <@ (10 + 2) * (10 + 2) @>

[<ReflectedDefinition>]
let foo a = a * a
    
expand Map.empty <@ foo (10 + 2) @>
