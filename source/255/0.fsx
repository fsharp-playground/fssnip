open System 
module List = 
    let insertAt index element input = 
        input |> List.mapi (fun i el -> if i = index then [element; el] else [el])
              |> List.concat       
        
let addSpaces (tokens: string list) = 
    [ for i = 0 to tokens.Length - 1 do
        if not <| String.IsNullOrEmpty(tokens.[i]) then
            yield List.insertAt i "" tokens
      yield tokens @ [""] ]

let addSpacesToAll (tokenLists: Set<string list>) =
    tokenLists
    |> Set.map (fun tokenList -> addSpaces tokenList)
    |> List.concat
    |> Set.ofList
