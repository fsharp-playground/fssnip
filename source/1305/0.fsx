open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

let notImplemented () = raise (System.NotImplementedException())

let toTypeName  t =
    if t = typeof<int> then "int"
    elif t = typeof<double> then "double" 
    elif t = typeof<bool> then "boolean"
    else notImplemented()

let rec toJava = function
   | Int32 x -> sprintf "%i" x
   | Double x -> sprintf "%gd" x
   | Bool true -> "true"
   | Bool false -> "false"
   | SpecificCall <@@ (+) @@> (None, _, [lhs;rhs]) -> toOp "+" lhs rhs
   | SpecificCall <@@ (-) @@> (None, _, [lhs;rhs]) -> toOp "-" lhs rhs
   | SpecificCall <@@ (*) @@> (None, _, [lhs;rhs]) -> toOp "*" lhs rhs
   | SpecificCall <@@ (/) @@> (None, _, [lhs;rhs]) -> toOp "/" lhs rhs
   | SpecificCall <@@ (=) @@> (None, _, [lhs;rhs]) -> toOp "==" lhs rhs
   | SpecificCall <@@ (<>) @@> (None, _, [lhs;rhs]) -> toOp "!=" lhs rhs
   | IfThenElse(condition, t, f) -> toIfThenElse condition t f
   | _ -> notImplemented()    
and toOp op lhs rhs =
    sprintf "(%s %s %s)" (toJava lhs) op (toJava rhs)
and toIfThenElse condition t f =
    sprintf "%s ? %s : %s" (toJava condition) (toJava t) (toJava f)

let toClass (expr:Expr<'TRet>)  =
    let returnType = toTypeName typeof<'TRet>
    let body = toJava expr
    sprintf """
public class Generated {
   public static %s fun(){
     return %s;
   }
   public static void main(String []args){
      System.out.println(fun());
   }
}""" returnType body

toClass <@ if 1 + 1 = 2 then 1 else -1 @>

(* Returns
public class Generated {
   public static int fun(){
     return ((1 + 1) == 2) ? 1 : -1;
   }
   public static void main(String []args){
      System.out.println(fun());
   }
}
*)