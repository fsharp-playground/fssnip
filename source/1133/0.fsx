// Remove block comment(s) from a list of tokens
let removeComments listIn start stop =
    let rec loop listIn listOut depth =   
        match listIn with
        | [] -> match depth with
                | 0 -> listOut
                | depth when depth > 0 -> failwith (sprintf "Too many '%s' tokens\n" stop)
                | _ -> failwith (sprintf "Too many '%s' tokens\n" start)
        | head::tail when depth > 0    -> loop [] listOut depth
        | head::tail when head = start -> loop tail listOut (depth - 1)
        | head::tail when head = stop  -> loop tail listOut (depth + 1)
        | head::tail when depth = 0    -> loop tail (head::listOut) depth
        | head::tail                   -> loop tail listOut depth
    loop listIn [] 0 |> List.rev

 // Tokenize string by splitting at whitespace and removing empty cells
let tokenize (t:string) = t.Split () |> Array.toList |> List.filter (fun x -> x <> "")

let source = ": CLSB ( n -- n ) dup 1 - and ; ( Clear least significant bit )"
printfn "Source: %A" source

let tokens = tokenize source
let noCommentTokens = removeComments tokens "(" ")"
printfn "Clean source %A" (List.fold (fun a t -> a + t + " ") " " noCommentTokens)