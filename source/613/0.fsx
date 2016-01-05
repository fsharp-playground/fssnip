open System
open System.Reflection
open Microsoft.FSharp.Quotations
open Patterns
open DerivedPatterns

// your implementation could be here =)
let mduration settlement maturity coupon yld frequency basis = 0M
let accrint issue first_interest settlement rate par frequency basis = 0M

// [snippet:Transform expressions into Excel formulae]
/// Simplified SpecificCall
let inline (|Func|_|) expr =(*[omit:(...)]*)
    match expr with
    | Lambdas(_,(Call(_,minfo1,_))) -> function
        | Call(obj, minfo2, args) when minfo1.MetadataToken = minfo2.MetadataToken ->
            Some args
        | _ -> None
    | _ -> failwith "invalid template parameter"(*[/omit]*)

/// Generate a formula pattern for an expression
/// Column number - index of var + 1 in the env
/// '#' is a temp placeholder for a row number
let generatePattern expr env =     
    // print binary ops: (x op y)
    let rec inline printBinaryOp op x y parens = 
        let res = transform x + op + transform y in if parens then "(" + res +  ")" else res

    // print functions: name(arg1, arg2, ...)
    and inline printFunc name args =
        let argValues: string[] = List.map transform args |> List.toArray
        sprintf "%s(%s)" name (String.Join (", ", argValues))

    // transform an expression into pattern
    and transform expr = 
        match expr with
        | Func <@@ (+) @@> [x; y] -> printBinaryOp "+" x y true
        | Func <@@ (-) @@> [x; y] -> printBinaryOp "-" x y true
        | Func <@@ (*) @@> [x; y] -> printBinaryOp "*" x y true
        | Func <@@ (/) @@> [x; y] -> printBinaryOp "/" x y true
        | Func <@@ ( ** ) @@> [x; y] -> printBinaryOp "^" x y false
        | Func <@@ (~-) @@> [x] -> "-" + transform x
        | Func <@@ (~+) @@> [x] -> transform x
        | Func <@@ mduration @@> args -> printFunc "MDURATION" args
        | Func <@@ accrint @@> args -> printFunc "ACCRINT" args
        | Lambdas (_, e) -> transform e
        | Value (v, _) -> string v
        // try to replace a varname with its column index
        | Var var -> 
            match List.tryFindIndex ((=)var.Name) env with
            | Some ind -> "R#C" + string (ind + 1)
            | _ -> var.Name
        // args.[i] means reference to the (i+1)th column
        | Call(None, mi, _::[Value (i, _)]) when mi.DeclaringType.Name = "IntrinsicFunctions" 
                                              && mi.Name = "GetArray" -> 
            let ind = unbox i in "R#C" + string (ind + 1)
        // replace MakeDecimal with a value
        | Call(None, mi, Value (v, _)::_) when mi.DeclaringType.Name = "IntrinsicFunctions" 
                                            && mi.Name = "MakeDecimal" -> 
            string v
        // DateTime ctor -> Excel DATE function: DATE(year, month, day)
        | NewObject(ci, Value(y,_)::Value(m,_)::Value(d,_)::_) when ci.DeclaringType.Name="DateTime"-> 
            sprintf "DATE(%A, %A, %A)" y m d
        | _ -> failwith (sprintf "Unknown expression type: %A" expr)

    "=" + transform expr

(*[omit:(Patterns example)]*)
// "=((((1+2^3)*3)-4)/5)"
generatePattern <@@ ((1. + 2.**3.) * 3. - 4.) / 5. @@> [] 
// "=(-x+1)"
generatePattern <@@ fun (x: decimal) -> -x + +1M @@> [ ] 
// "=R#C1^4"
generatePattern <@@ fun a b -> a ** 4. @@> ["a"; "b"] 
// "=(R#C1+R#C2)" - arrays can be used instead of explicit var names
generatePattern <@@ fun (args: _ array) -> args.[0] + args.[1] @@> []
// "=ACCRINT(R#C1, DATE(2010, 9, 8), R#C2, 10, 100, 2, 0)"
generatePattern <@@ fun issue settlement -> 
    accrint issue (DateTime(2010,9,8)) settlement 10 100 2 0 @@> [ "issue"; "settlement"](*[/omit]*)

module Test =
    [<ReflectedDefinition>]
    let sum (a: decimal) b = a + b
  
    [<ReflectedDefinition>]
    let mdurationMonth m c y f basis = (mduration (DateTime(2012, 1, 7)) m c y f basis) * 12M

    let run export = 
        let data: obj list list =(*[omit:(Some data)]*)[
            [ 42; 0; DateTime(2012, 1, 7); DateTime(2030, 1, 1); 15M; 0.9M; 1; 1 ]
            [ null; null; null; DateTime(2016, 1, 7); 8M; 9M; 2; 1 ]
        ](*[/omit]*)

        // the vars with such names will be replaced with R{rownum}C{var index + 1}
        let dataColumns = ["a"; "b"; "s"; "m"; "c"; "y"; "f"; "basis"]
        let funcs = ["sum"; "mdurationMonth"]

        // try to find the reflected definitions (usual expressions can be used instead)
        let reflectedDefinitions = 
            let methods = 
                Assembly.GetExecutingAssembly().GetTypes() 
                |> Array.collect (fun t -> t.GetMethods())

            Array.foldBack (fun mi state -> 
                match Expr.TryGetReflectedDefinition mi with
                | Some expr -> (mi.Name, expr) :: state
                | None -> state) methods []
            |> Map.ofList

        // transform quotation into a pattern & split by the rownum replacement '#'
        let unquote = 
            Option.bind (fun expr -> 
                try 
                    Some ((generatePattern expr dataColumns).Split [|'#'|])
                with _ -> None)

        // reflected definitions -> formulae
        let formulae = funcs
                        |> List.map (reflectedDefinitions.TryFind >> unquote)
                        |> List.filter Option.isSome
                        |> List.map Option.get
        
        data |> List.iteri (export formulae)

/// Export data with given pattern
let export exportValue exportFunc (patterns: string[] list) row (items: _ list) =(*[omit:(...)]*)
    let row = row + 1
    let j = items.Length
    let formulae = patterns |> List.map (fun arr -> String.Join (string row, arr))
    List.iteri (exportValue row) items
    List.iteri (fun i formula -> 
        let cell = sprintf "%c%d" (char (65 + i + j)) row
        exportFunc cell formula) formulae(*[/omit]*)

// Standard output
Test.run (export 
            (fun row i item -> printfn "Cells.[%d, %d]<-%A" row (i+1) item)
            (fun cell formula -> printfn "Range(%A, %A).Formula <- \"%s\"" cell cell formula))
// [/snippet]

#if INTERACTIVE
#r "Microsoft.Office.Interop.Excel"
#endif

open Microsoft.Office.Interop.Excel
// [snippet:Simple export to Excel]
let app = new ApplicationClass()
let workbook = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet)
let worksheet = workbook.Worksheets.[1] :?> Worksheet

// fill the cells
Test.run (export 
            (fun row i item -> worksheet.Cells.[row, i+1] <- item)
            (fun cell formula -> worksheet.Range(cell, cell).Formula <- formula))

app.ReferenceStyle <- XlReferenceStyle.xlA1

(*[omit:(Close workbook and release objects)]*)
// release COM objects
let inline release (objs: obj list) = 
    List.iter (System.Runtime.InteropServices.Marshal.ReleaseComObject >> ignore) objs

let filename = "test.xls"
workbook.SaveAs(filename, XlFileFormat.xlWorkbookNormal)
workbook.Close true
app.Quit()

release [worksheet; workbook; app]
(*[/omit]*)
// [/snippet]