// [snippet:definition of the tree]

type MultiTree<'a> =
    | Leaf of 'a
    | Node of MultiTree<'a> list

// [/snippet]

// [snippet:addition]

let sumMultiTree tree =
    let rec sumTree node cont =
        match node with
        | Leaf i -> cont i
        | Node chs -> 
            match chs with
            | [] -> cont 0
            | n::ns' -> sumTree n (fun e -> sumTree (Node ns') (fun e' -> cont (e + e')))
    sumTree tree (fun r -> r)

// [/snippet]

// [snippet:general operation]

let opMultiTree f neutral tree =
    let rec opTree node cont =
        match node with
        | Leaf a -> cont a
        | Node chs -> 
            match chs with
            | [] -> cont neutral
            | n::ns' -> opTree n (fun e -> opTree (Node ns') (fun e' -> cont (f e e')))
    opTree tree (fun r -> r)

let sumMultiTree' = opMultiTree (+) 0

// [/snippet]