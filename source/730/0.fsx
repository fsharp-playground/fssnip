module Parsing =
    open System.Text.RegularExpressions

    let (|QuotedId|_|) (s : string) =
        let s = s.TrimStart()
        if s.StartsWith("\"") then
            match s.IndexOf('"', 1) with
            | -1 -> None
            | x -> Some(s.[1 .. x - 1], s.Substring(x + 1))
        else
            None

    let idRegex = Regex("^[a-zA-Z_][a-zA-Z_0-9]*")

    let (|UnquotedId|_|) (s : string) =
        let s = s.TrimStart()
        let m = idRegex.Match(s)
        if m.Success then
            Some(m.Value, s.Substring(m.Value.Length))
        else
            None

    let (|Id|_|) s =
        match s with
        | QuotedId(id, rest)
        | UnquotedId(id, rest) -> Some(id, rest)
        | _ -> None

    let (|TL|_|) prefix (s : string) =
        let s = s.TrimStart()
        if s.StartsWith(prefix) then
            Some (s.Substring(prefix.Length))
        else
            None

    let rec (|Params|_|) s =
        match s with
        | Id(x, TL ")" rest) -> Some ([x], ")" + rest)
        | Id(x, TL "," (Params(pars, rest))) -> Some (x :: pars, rest)
        | _ -> None

    type Expr =
        | AllExpr of string * Expr
        | SomeExpr of string * Expr
        | FuncExpr of string * string list
        | AndExpr of Expr * Expr
        | OrExpr of Expr * Expr
        | ImplExpr of Expr * Expr
        | EquivExpr of Expr * Expr
        | NegExpr of Expr
        | IdExpr of string
        | TrueExpr
        | FalseExpr

    let rec (|BaseE|_|) s =
        match s with
        | TL "true" rest -> Some (TrueExpr, rest)
        | TL "false" rest -> Some (FalseExpr, rest)
        | TL "~" (Id(id, rest)) -> Some (NegExpr (IdExpr id), rest)
        | TL "~" (BaseE(e, rest)) -> Some (NegExpr e, rest)
        | TL "ALL" (Id(id, BaseE(e, rest))) -> Some (AllExpr(id, e), rest)
        | TL "SOME" (Id(id, BaseE(e, rest))) -> Some (SomeExpr(id, e), rest)
        | TL "(" (Expr(e, TL ")" rest)) -> Some (e, rest)
        | Id(id, TL "(" (Params(pars, TL ")" rest))) -> Some (FuncExpr(id, pars), rest)
        | Id(id, rest) -> Some (IdExpr id, rest)
        | _ -> None

    and (|AndE|_|) s =
        match s with
        | BaseE(e1, TL "&" (AndE(e2, rest))) -> Some (AndExpr(e1, e2), rest)
        | BaseE(e, rest) -> Some(e, rest)
        | _ -> None

    and (|OrE|_|) s =
        match s with
        | AndE(e1, TL "#" (OrE(e2, rest))) -> Some (OrExpr(e1, e2), rest)
        | AndE(e, rest) -> Some(e, rest)
        | _ -> None

    and (|ImplE|_|) s =
        match s with
        | OrE(e1, TL "->" (ImplE(e2, rest))) -> Some (ImplExpr(e1, e2), rest)
        | OrE(e, rest) -> Some(e, rest)
        | _ -> None

    and (|EquivE|_|) s =
        match s with
        | ImplE(e1, TL "<->" (ImplE(e2, rest))) -> Some (EquivExpr(e1, e2), rest)
        | _ -> None

    and (|Expr|_|) s =
        match s with
        | EquivE(e, rest)
        | ImplE(e, rest) -> Some(e, rest)
        | _ -> None


module ParsingTests =
    open Parsing

    match "ALL x (SOME y (pred(x, y) -> (f(x) <-> g(x, y))))" with
    | Expr(e, "") -> printfn "%A" e
    | _ -> printfn "Failed"

    match "A & ~(B # C) & D # E" with
    | Expr(e, "") -> printfn "%A" e
    | _ -> printfn "Failed"

    match "ALL x A & B" with
    | Expr(e, "") -> printfn "%A" e
    | _ -> printfn "Failed"

    match "ALL x (A & B)" with
    | Expr(e, "") -> printfn "%A" e
    | _ -> printfn "Failed"