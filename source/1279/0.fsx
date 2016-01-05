open System

let numbers = "
 _     _ __     _  _  _  _  _ 
| |  | _|__||_||_ |_   ||_||_|
|_|  ||_ __|  | _||_|  ||_| _|
"

let buildGroups(numbers : string) =
    let xss =
        numbers.Split([|'\n';'\r'|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun cs -> cs.ToCharArray())
    [|
        for i = 0 to (xss.[0].Length - 1) / 3 do
            yield [|
                for j = 0 to 2 do
                    for k = 0 to 2 do  
                        yield xss.[j].[k + i * 3]
            |]
    |]
    
let lookup =
    buildGroups numbers |> Array.mapi (fun i xs ->
        xs, i) |> Map.ofArray

let classify =
    buildGroups >> Array.map (fun xs -> lookup.[xs])

let testCase = """
 _     _ __ 
| |  | _|__|
|_|  ||_ __|
"""

classify testCase
