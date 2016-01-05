


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

let get_row idx = idx / 9
let get_col idx = idx % 9
let get_box idx =
    let r = get_row idx / 3
    let c = get_col idx / 3
    3 * r + c

let get_neighbors idx =
    let r = get_row idx
    let c = get_col idx
    let b = get_box idx
    [0 .. 80]
    |> List.filter (fun x ->
        x <> idx && 
                    (get_row x = r || get_col x = c || get_box x = b))

let squares = [|0 .. 80|]

let unit_list =
    Array.concat [[|0 .. 8|]
                  |> Array.map (fun r -> Array.filter (fun x -> get_row x = r) squares);
                  [|0 .. 8|]
                  |> Array.map (fun r -> Array.filter (fun x -> get_col x = r) squares);
                  [|0 .. 8|]
                  |> Array.map (fun r -> Array.filter (fun x -> get_box x = r) squares)]

let units =
    squares
    |> Array.map (fun idx ->
        unit_list
        |> Array.filter (fun xs ->
            xs
            |> Array.exists (fun x -> x = idx)))

let neighbor_table =
    [|0 .. 80|]
    |> Array.map get_neighbors


type Board = string []

let initial_board () = Array.create 81 "123456789" 

let sudoku_show (board:Board) =
    for r in 0 .. 8 do
        if r = 3 || r = 6 then
            printfn "-------------------"
        for c in 0 .. 8 do
            if c = 3 || c = 6 then
                printf " |"
            printf " %A" (board.[r * 9 + c])
        printfn ""


let rec assign (board:Board) idx value = 
    board.[idx].Replace(value, "")
    |> String.forall (fun c -> eliminate board idx (string c))

and eliminate board idx value =
    if not (board.[idx].Contains(value)) then
        true
    else
        board.[idx] <- board.[idx].Replace(value, "")

        if board.[idx].Length = 0 then
            false // cannot set here
        else
            let r1 = if board.[idx].Length = 1 then
                         let v = board.[idx]
                         (neighbor_table.[idx]
                         |> Seq.forall (fun i ->
                                         eliminate board i v))
                     else true

            let r2 = r1 &&
                     units.[idx]
                     |> Array.forall (fun un ->
                          let dplaces = un |>
                                           Array.filter (fun x -> board.[x].Contains(value))
                          if Array.isEmpty dplaces then
                             false
                          elif Array.length dplaces = 1 then
                             assign board dplaces.[0] value
                          else true)
            r2


let val_count (board:Board) idx =
    board.[idx].Length

let sudoku_next_2 (board:Board) = 
    let best_count, best_idx =
        seq { 0 .. 80 }
        |> Seq.map (fun idx ->
            val_count board idx, idx)
        |> Seq.filter (fun (c, idx) -> c <> 1)
        |> Seq.min
        // printfn "best_idx: %A, best_count: %A" best_idx best_count
    seq {
        for v in board.[best_idx]  do
            let board' = Array.copy board
            if assign board' best_idx (string v) then
                yield board'
        }

let sudoku_done_2 (board:Board) =
    board |> Array.forall (fun x -> x.Length = 1)

let sudoku_read str =
    let board = initial_board()

    Seq.fold (fun idx c ->
        if c = '.' || c = '0' then
              idx + 1
        elif c < '1' || c > '9' then
              idx
        else
            assign board idx (string c) |> ignore
            idx + 1) 0 str
    |>  fun  c ->
        if c <> 81 then failwithf "%d chars read" c
        else
            board
let sudoku board =
    board
    |> sudoku_read
    |> solve sudoku_next_2 sudoku_done_2
    |> Seq.head
    |> sudoku_show
    
let ex2 = "003020600900305001001806400008102900700000008006708200002609500800203009005010300"

sudoku ex2