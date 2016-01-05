open System

let append (a:string) (lst:string list) = 
    lst |> List.collect (fun ch -> if a.Contains(ch) then [] else [a+ch])

let generateCombination (lst:string list) = 
    let total = lst.Length
    let rec combination (acc:string list list) (src:string list) (target: string list) = 
        let result = [for i in src -> target |> append i] |> List.collect id
        match result.Head.Length with
        | x when x = total -> result::acc
        | _ -> combination (result::acc) result target

    combination [lst] lst lst
  
for str in generateCombination [for i in "abc" -> i.ToString()] |> List.rev |> List.collect id do
    str |> Console.WriteLine 

// Input: "ab"
// output: "a" "b" "ab" "ba"