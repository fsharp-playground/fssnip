open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

let notImplemented () = raise (System.NotImplementedException())

let toTypeName  t =
    if t = typeof<int> then "int"
    elif t = typeof<double> then "double" 
    elif t = typeof<bool> then "boolean"
    else notImplemented()

let name =
    let x = ref 0
    fun () -> incr x; sprintf "_%d" !x

let rec toJava (add:string -> unit) = function
   | Let(var, expr, body) -> toLet add var expr body
   | Var(var) -> sprintf "%s" var.Name
   | Int32 x -> sprintf "%i" x
   | Double x -> sprintf "%gd" x
   | Bool true -> "true"
   | Bool false -> "false"
   | SpecificCall <@@ (+) @@> (None, _, [lhs;rhs]) -> toArithOp add "+" lhs rhs
   | SpecificCall <@@ (-) @@> (None, _, [lhs;rhs]) -> toArithOp add "-" lhs rhs
   | SpecificCall <@@ (*) @@> (None, _, [lhs;rhs]) -> toArithOp add "*" lhs rhs
   | SpecificCall <@@ (/) @@> (None, _, [lhs;rhs]) -> toArithOp add "/" lhs rhs
   | SpecificCall <@@ (=) @@> (None, _, [lhs;rhs]) -> toLogicOp add "==" lhs rhs
   | SpecificCall <@@ (<>) @@> (None, _, [lhs;rhs]) -> toLogicOp add "!=" lhs rhs
   | IfThenElse(condition, t, f) -> toIfThenElse add condition t f
   | _ -> notImplemented()
and toLet add var expr body =
    let valueName = toJava add expr
    add <| sprintf "%s %s = %s;" (toTypeName var.Type) var.Name valueName
    toJava add body
and toArithOp add op lhs rhs =
    let l,r = (toJava add lhs), (toJava add rhs)
    let name = name ()
    add <| sprintf "%s %s = (%s %s %s);" (toTypeName lhs.Type) name l op r
    name
and toLogicOp add op lhs rhs =
    let l,r = (toJava add lhs), (toJava add rhs)
    let name = name ()
    add <| sprintf "boolean %s = (%s %s %s);" name l op r
    name
and toIfThenElse add condition t f =
    let cn, tn, fn = toJava add condition, toJava add t, toJava add f
    let name = name ()
    add <| sprintf "%s %s = %s ? %s : %s;" (toTypeName t.Type) name cn tn fn
    name

let toClass (expr:Expr<'TRet>)  =
    let returnType = toTypeName typeof<'TRet>
    let w = System.Text.StringBuilder()
    let add s = w.AppendLine("    " + s) |> ignore
    let v = toJava add expr
    sprintf """
public class Generated {
  public static %s fun(){
%s    return %s;
  }
  public static void main(String []args){
    System.out.println(fun());
  }
}"""  returnType (w.ToString()) v

toClass <@ let x = 1 in let y = 1 in if x + y = 2 then 1. else -1. @>

(*Returns

public class Generated {
  public static double fun(){
    int x = 1;
    int y = 1;
    int _1 = (x + y);
    boolean _2 = (_1 == 2);
    double _3 = _2 ? 1d : -1d;
    return _3;
  }
  public static void main(String []args){
    System.out.println(fun());
  }
}

*)