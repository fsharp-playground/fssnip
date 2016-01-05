open System.Text

let printBytes b =
    printfn "%s" (Encoding.ASCII.GetString(b))

let bytes = [| 104uy; 101uy; 108uy; 108uy; 111uy |]
printBytes bytes // prints "hello"

let bytes2 = "hello"B
printBytes bytes2 // also prints "hello"