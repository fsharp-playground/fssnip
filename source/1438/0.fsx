let myArray = [|1;5;3;7;2|]
let sortedArray = Array.sort myArray
printfn "%A" [| for i in sortedArray -> Array.findIndex (fun x-> x = i) myArray |]

//trying to condense
Array.sort myArray |> Array.map(fun b-> Array.findIndex (fun a-> b=a myArray)) |> printfn "%A" 