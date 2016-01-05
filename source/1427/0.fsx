#r "System.Data.Linq" 

open System
open System.Reflection
open System.ComponentModel
open System.Linq.Expressions
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations

module FSharpType = 
    let IsOption (stype: System.Type) = stype.Name = "FSharpOption`1"

module RecordCloning = 
    let inline application prms expr = Expr.Application(expr, prms)
    let inline coerse typ expr = Expr.Coerce(expr, typ)
    let inline newrec typ args = Expr.NewRecord(typ, args)

    let (|IsMapType|_|) (t: Type) = 
        if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Map<_,_>> then Some t
        else None    

    let rec copyThing (mtype: Type) : Expr = 
        match mtype with 
        | _ when FSharpType.IsRecord mtype -> genRecordCopier mtype
        | _ when FSharpType.IsUnion mtype  -> genUnionCopier mtype 
        | _ when mtype.IsValueType || mtype = typeof<String> -> getIdFunc mtype
        | _ when mtype.IsArray -> genArrayCopier mtype
        | IsMapType t -> getIdFunc mtype
        | _ when mtype = typeof<System.Object> -> getIdFunc mtype
        | _ -> failwithf "Unexpected Type: %s" (mtype.ToString())

    and X<'T> : 'T = Unchecked.defaultof<'T>

    and getMethod = 
        function
        | Patterns.Call (_, m, _) when m.IsGenericMethod -> m.GetGenericMethodDefinition()
        | Patterns.Call (_, m, _) -> m
        | _ -> failwith "Incorrect getMethod Pattern"

    and getIdFunc itype =
        let arg = Var("x", itype, false)
        let argExpr = Expr.Var(arg)        
        let func = 
            let m = (getMethod <@ id X @>).MakeGenericMethod([|itype|])
            Expr.Call(m, [argExpr])
        Expr.Lambda(arg, func)

    and genRecordCopier (rtype: Type) : Expr =         
        let arg = Var("x", rtype, false)
        let argExpr = Expr.Var(arg) 
        let newrec =            
            FSharpType.GetRecordFields(rtype) |> Array.toList
            |> List.map (fun field -> genFieldCopy argExpr field)
            |> newrec rtype
        Expr.Lambda(arg, newrec)

    and genFieldCopy argExpr (field: PropertyInfo) : Expr = 
        let pval = Expr.PropertyGet(argExpr, field) 
        copyThing field.PropertyType |> application pval
             
    and genArrayCopier (atype : Type) : Expr = 
        let etype = atype.GetElementType()        
        let copyfun = copyThing etype

        let arg = Var("arr", atype, false)
        let argExpr = Expr.Var(arg)
        
        let func =
            let m = (getMethod <@ Array.map X X @>).MakeGenericMethod([|etype; etype|])
            Expr.Call(m, [copyfun; argExpr])

        Expr.Lambda(arg, func)

    and genUnionCopier (utype: Type) : Expr = 
        let cases = FSharpType.GetUnionCases utype
        // if - union case - then - copy each field into new case - else - next case

        let arg = Var("x", utype, false)
        let useArg = Expr.Var(arg)

        let genCaseTest case = Expr.UnionCaseTest (useArg, case)
        
        let makeCopyCtor (ci: UnionCaseInfo) = 
            let copiedMembers = [ for field in ci.GetFields() -> genFieldCopy useArg field ]
            Expr.NewUnionCase(ci, copiedMembers)

        let genIf ifCase thenCase elseCase = Expr.IfThenElse(ifCase, thenCase, elseCase)

        let typedFail (str: string) =
            let m = (getMethod <@ failwith str @>).MakeGenericMethod([|utype|])
            Expr.Call(m, [ <@ str @> ])
                                                         
        let nestedIfs = 
            cases
            |> Array.map (fun case -> genIf (genCaseTest case) (makeCopyCtor case))
            |> Array.foldBack (fun iff st -> iff st) <| (typedFail "Unexpected Case in Union")

        Expr.Lambda(arg, nestedIfs)
       
    let toLinq<'I,'O> (expr: Expr<'I -> 'O>) =
        let linq = Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.QuotationToExpression expr
        let call = linq :?> MethodCallExpression
        let lambda  = call.Arguments.[0] :?> LambdaExpression
        Expression.Lambda<Func<'I,'O>>(lambda.Body, lambda.Parameters)

    let genrateRecordDeepCopyFunction<'T> () : ('T -> 'T) = 
        let expr = genRecordCopier typeof<'T> 
        let castExpr : Expr<'T -> 'T> = expr |> Expr.Cast
        let compiledExpr = (castExpr |> toLinq).Compile()
        fun (v : 'T) -> compiledExpr.Invoke(v)
