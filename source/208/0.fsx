open System.Text

let bytes = [| 104uy; 101uy; 108uy; 108uy; 111uy |]
printfn "%s" (Encoding.ASCII.GetString(bytes))