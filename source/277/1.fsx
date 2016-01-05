    let rec permutations (A : 'a list) =
        if List.isEmpty A then [[]] else
        [
            for a in A do
            yield! A |> List.filter (fun x -> x <> a) 
                     |> permutations
                     |> List.map (fun xs -> a::xs)
        ]