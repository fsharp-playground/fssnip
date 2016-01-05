let solve next_f done_f initial =
    let rec go state =
        seq {
            if done_f state then
               yield state
            else
               for state' in next_f state do
                   yield! go state'
            }
    go initial



let n_queens n =
    let all_rows = Set.ofList [1..n]
    let next_f queens =
        let column = List.length queens + 1
        queens
        |> List.fold (fun possible (row, col) ->
            let distance = column - col
            // Remove the current row, and diagonals from the
            // possible set.  The diagonals are just previous
            // row +/- the column difference. This is safe here,
            // because even if row - distance is negative, we are
            // removing it from another set.  No array access is
            // involved.
            Set.difference possible
                      (Set.ofList [row; row + distance;
                                   row - distance])) all_rows
        |> Set.map (fun (row) ->
                (row, column)::queens)
    let done_f queens =
        List.length queens = n

    solve next_f done_f []

printfn "number of 8 queens: %A" (Seq.length (n_queens 8))
printfn "number of 9 queens: %A" (Seq.length (n_queens 9))
printfn "number of 10 queens: %A" (Seq.length (n_queens 10))

printfn "A solution to 20 queens: %A" (Seq.head (n_queens 20))
