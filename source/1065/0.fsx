module List =
    // single case ap combinator
    let (|Map|) (f : 'T -> 'S) ts = List.map f ts

    // partial case ap combinators
    let (|Choose|) (f : 'T -> 'S option) ts = List.choose f ts
    let (|TryMap|_|) (f : 'T -> 'S option) (ts : 'T list) =
        let rec aux acc ts =
            match ts with
            | [] -> List.rev acc |> Some
            | t :: rest ->
                match f t with 
                | Some s -> aux (s :: acc) rest
                | None -> None

        aux [] ts

// a trivial example

let (|NonNegative|_|) (x : int) = if x >= 0 then Some x else None

let test =
    function
    | Some (List.TryMap (|NonNegative|_|) values) -> values
    | Some (List.Choose (|NonNegative|_|) values) ->
        printfn "found some non-negative values in inputs"
        values
    | _ -> []

test <| Some [-10..10]
test <| Some [1..10]