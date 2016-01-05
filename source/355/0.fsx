
open Microsoft.FSharp.Quotations

module P = Quotations.Patterns

type Comparison
  = Equals of string * obj
  | NotEquals of string * obj

let getNameValuePair (expr:Expr<('a -> bool)>) =
  match expr with
  | P.Lambda(_, P.Call(None, mi, [Patterns.PropertyGet(_, pi, []); P.Value(value, _) ])) -> 

    match mi.Name with
    | "op_Equality" ->Some(Equals(pi.Name, value))
    | "op_Inequality" -> Some(NotEquals(pi.Name, value))
    | _ -> None

  | _ -> None
  
let test = <@ fun (s:string) -> s.Length <> 2 @>

let r = getNameValuePair test