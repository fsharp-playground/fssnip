let wins = [[1;2;3];
            [4;5;6];
            [7;8;9];
            [1;4;7];
            [2;5;8];
            [3;6;9];
            [1;5;9];
            [3;5;7]]

let Contains number = List.exists (fun n -> n = number)

let ContainsList list = List.forall (fun n -> list |> Contains n)

let ExceptList list = List.filter (fun n -> not (list |> Contains n))

let Available (player: int list) (opponent: int list) =
    [1..9]
    |> ExceptList (List.append player opponent)  

let IsWin (squares: int list) = 
    wins |> List.exists (fun w -> ContainsList squares w)

let IsDraw player opponent =
    List.isEmpty (Available player opponent)

let rec Score (player: int list) (opponent: int list) =
    if (IsWin player) then 1
    else if (IsDraw player opponent) then 0
    else
        let opponentsBestMove = BestMove opponent player
        let opponentsNewPosition = opponentsBestMove::opponent
        -Score opponentsNewPosition player

and BestMove (player: int list) (opponent: int list) =
    Available player opponent
    |> List.maxBy (fun m -> Score (m::player) opponent)