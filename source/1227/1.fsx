open System
open System.Linq

let marks = [","; "."; "?"; "!"; "?!"]
let rnd = new Random()

let rec insert v i l =
    match i, l with
    | 0, h -> v::h
    | i, h::t -> h::insert v (i - 1) t
    | _, [] -> failwith "index out of range"

let rec remove i l =
    match i, l with
    | 0, h::t -> t
    | i, h::t -> h::remove (i - 1) t
    | _, [] -> failwith "index out of range"

let shuffleWord (input:string) =
    let result = Array.create input.Length '\000'
    printf "%A| " input
    let rec _shuffle (inp:list<char>) ix =
        match ix with
        | 0 -> [inp.[0]]
        | _ ->
            let idx = rnd.Next(inp.Length - 1)
            inp.[idx] :: _shuffle (remove idx inp) (ix - 1)

    let w = 
        match input.Length with
        | l when l = 1 -> [input.[0]]
        | l when l < 3 -> [input.[0]; input.[1]]
        | l when l = 3 -> [input.[0]; input.[2]; input.[1]]
        | _ -> 
            let midPart = input.[1 .. input.Length - 2] |> List.ofSeq
            input.[0] :: (_shuffle midPart (midPart.Length - 1 )) @ [input.Last()]

    w
    |> List.map (fun (c:char) -> c.ToString())
    |> List.reduce (+)

let rec shuffle (input:string) =
    let rec _clear (input:string) marks =
        match marks with
        | [] -> [input]
        | h::t -> 
            if input.EndsWith(h) then (input.Substring(0, input.Length - 1)) :: [h]
            else _clear input t

    let rec _shuffle list =
        match list with
        | [] -> []
        | h::t -> ((_clear h marks) |> List.map shuffleWord) @ _shuffle t

    let arrayOfWords = (input.Split([|" "|], StringSplitOptions.None) |> List.ofArray)
    _shuffle arrayOfWords |> List.reduce (fun a b -> a + " " + b)


shuffle "By the way code below was used as brain practice to shuffle this text."