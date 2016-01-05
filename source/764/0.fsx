// Example: mergeBy (fun (key,_)->key) [("a", 1); ("c", 2)] [("c", 2); ("d", 3)];;
// val it : ((string * int) option * (string * int) option) list =
// [(Some ("a", 1), None); (Some ("c", 2), Some ("c", 2)); (None, Some ("d", 3))]

let mergeBy keyselector ls rs = 
    let rec aux ls rs acc =
        match ls, rs with
        | [], [] -> acc
        | l::ls', [] -> aux ls' [] ((Some l, None)::acc)
        | [], r::rs' -> aux [] rs' ((None, Some r)::acc)
        | l::ls', r::rs' -> 
            match compare (keyselector l) (keyselector r) with
            | n when n < 0 -> aux ls' rs ((Some l, None)::acc)
            | 0 -> aux ls' rs' ((Some l, Some r)::acc)
            | _ -> aux ls rs' ((None, Some r)::acc)
            
    aux ls rs [] |> List.rev