open System

let isPal str = 
    let str = str |> Seq.filter ((<>) ' ') |> Seq.toList
    str = (str |> List.rev)

["abc ba"; "abcba"; "a"; "aaba"; ""] 
|> List.map (fun x -> x, isPal x)