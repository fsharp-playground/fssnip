//[snippet: Clojure-like syntax sugar]
//needs to install the F# PowerPack
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.ExprShape
open Microsoft.FSharp.Linq.QuotationEvaluation

let _0<'T> : 'T = failwith "placeholder"
let _1<'T> : 'T = failwith "placeholder"
let _2<'T> : 'T = failwith "placeholder"
let _3<'T> : 'T = failwith "placeholder"
let _4<'T> : 'T = failwith "placeholder"
let _5<'T> : 'T = failwith "placeholder"
let _6<'T> : 'T = failwith "placeholder"
let _7<'T> : 'T = failwith "placeholder"
let _8<'T> : 'T = failwith "placeholder"
let _9<'T> : 'T = failwith "placeholder"

let L (expr : Expr<'T>) : 'U =
  let rec iter acc  expr = 
    let f acc name ty =
      let var, acc = 
        acc
        |> List.tryFind (fun (v:Var) -> v.Name = name)
        |> function
        | Some(var) -> var, acc
        | None ->
          let var = Var(name, ty)
          var, var::acc
      Expr.Var(var), acc
    match expr with
    | ShapeVar(_) as v -> v, acc
    | ShapeLambda(var, expr) ->
      let expr, acc = iter acc expr
      Expr.Lambda(var, expr), acc
    | SpecificCall <@ _0 @> (_,[ty],_) -> f acc "_0" ty
    | SpecificCall <@ _1 @> (_,[ty],_) -> f acc "_1" ty
    | SpecificCall <@ _2 @> (_,[ty],_) -> f acc "_2" ty
    | SpecificCall <@ _3 @> (_,[ty],_) -> f acc "_3" ty
    | SpecificCall <@ _4 @> (_,[ty],_) -> f acc "_4" ty
    | SpecificCall <@ _5 @> (_,[ty],_) -> f acc "_5" ty
    | SpecificCall <@ _6 @> (_,[ty],_) -> f acc "_6" ty
    | SpecificCall <@ _7 @> (_,[ty],_) -> f acc "_7" ty
    | SpecificCall <@ _8 @> (_,[ty],_) -> f acc "_8" ty
    | SpecificCall <@ _9 @> (_,[ty],_) -> f acc "_9" ty
    | ShapeCombination(obj, []) ->
      RebuildShapeCombination(obj, []), acc
    | ShapeCombination(obj, exprs) -> 
      let exprs, acc =
        let e, a = iter acc (List.head exprs)
        List.tail exprs
        |> List.fold (fun (es, a) e ->
          let e, a = iter a e
          (e::es, a)
          ) ([e], a)
      let exprs = List.rev exprs
      RebuildShapeCombination(obj, exprs), acc
  let rec buildLambda acc expr =
    match acc with
    | [] -> expr
    | var::acc -> 
      Expr.Lambda(var, expr)
      |> buildLambda acc
  let expr, acc = iter [] expr
  let acc =
    acc |> List.sortWith (fun a b -> compare b a)
  let expr: Expr<'U> = Expr.Cast(buildLambda acc expr)
  expr.Compile() ()
//[/snippet]

//[snippet: examples]
L<@ _1 + _0 + _1 + _0 @> 1 2
|> printfn "%d"

let tuple : int * int =
  L<@ (_1 + _3, _0 + _2) @> 0 1 2 3
printfn "%A" tuple

[0..2]
|> List.map (L<@ (_0 + 1).ToString() @>)
|> List.reduce (+)
|> printfn "%s"
//[/snippet]