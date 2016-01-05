open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.ExprShape

type Expr with
    static member Erase (e : Expr<'T>) =
        match e with
        | ShapeVar v -> Expr.Var v
        | ShapeLambda (v, body) -> Expr.Lambda(v, body)
        | ShapeCombination (o, exprs) -> RebuildShapeCombination(o, exprs)


Expr.Erase <@ 1 + 1 @> |> fun e -> e.GetType()