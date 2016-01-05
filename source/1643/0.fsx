open Microsoft.FSharp.Quotations

let rec funName = function
| Patterns.Call(None, methodInfo, _) -> methodInfo.Name
| Patterns.Lambda(_, expr) -> funName expr
| _ -> failwith "Unexpected input"

let foo () = 42
funName <@ foo @>       // "foo"