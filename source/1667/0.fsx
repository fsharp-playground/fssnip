
type NestedType<'a> = List of NestedType<'a> list | E of 'a

let flatten ls =
    let rec start temp input = 
        match input with
        | E e -> e::temp
        | List es -> List.foldBack(fun x acc -> start acc x) es temp
    start [] ls

// test (a (b (c d) e)))
(List [E "a"; List [E "b" ; List [E "c";  E "d"]; E "e"]]) |> flatten = ["a";"b";"c";"d";"e";]