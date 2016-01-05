let array2DFlat (arr:'a [,]) =
    let rec array2DFlat (arr:'a [,]) i (acc:'a array) =
        match  Array2D.length1 arr with 
        | x when i = x -> acc
        | _ -> array2DFlat arr (i + 1) 
                           acc 
                           |> Array.append arr.[i, 0..(Array2D.length2 arr - 1)]
    array2DFlat arr 0 [||]