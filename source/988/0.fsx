let rec traverseQuotation f q = 
  f q
  match q with
  | Patterns.Call(inst, mi, args) ->
      match Expr.TryGetReflectedDefinition(mi) with
      | Some e -> traverseQuotation f e
      | _ -> ()
  | _ -> ()
  match q with
  | ExprShape.ShapeCombination(a, args) -> 
      args |> List.iter (traverseQuotation f)
  | ExprShape.ShapeLambda(v, body)  -> 
      traverseQuotation f body
  | ExprShape.ShapeVar(v) -> ()

let checkRecords = function
  | Patterns.NewRecord(r, args) -> 
     printfn "Record: %s" r.Name
  | Patterns.NewObject(ci, args) -> 
     if Microsoft.FSharp.Reflection.FSharpType.IsRecord(ci.DeclaringType) then
       printfn "boo! %s" ci.DeclaringType.Name 
     else 
       printfn "Object: %s" ci.DeclaringType.Name 
  | _ -> ()
      
traverseQuotation checkRecords <@@ main() @@> 
