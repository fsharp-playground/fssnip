// [snippet:Remove first ocurrence from list]
let rec remove_first pred lst =
    match lst with
    | h::t when pred h -> t
    | h::t -> h::remove_first pred t
    | _ -> []
// [/snippet]
// [snippet:Usage:]
let somelist = [('a',2);('f',7);('a',4);('h',10)]

let removed = somelist |> remove_first (fun (x,y) -> x='a')
// Result is:
// [('f',7);('a',4);('h',10)]
// [/snippet]
// [snippet:Remove nth ocurrence from list]
// This requires a small modification:
let rec remove_nth pred n lst =
    match lst, n with
    | (h::t, 1) when pred h -> t
    | (h::t, _) when pred h -> h::remove_nth pred (n-1) t
    | (h::t, _) -> h::remove_nth pred n t
    | _ -> []
// [/snippet]