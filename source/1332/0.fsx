let (|Array|_|) l a =
    let rec loop i =
        function
        | h :: tail when h = Array.get a i -> loop (i+1) tail
        | [] -> a |> Seq.skip i |> Seq.toArray |> Some
        | _ -> None
    loop 0 l

match [| 1;2;3;4;5;6 |] with
| Array [ 1;2] tail -> tail
| _ -> [||]

