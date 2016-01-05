    /// The two players
    type Player = A | B

    /// The point score in for a player in a game
    type PlayerPoints = Zero | Fifteen | Thirty | Forty 

    /// The score of a game
    type Score = 
        | Points of PlayerPoints * PlayerPoints 
        | Advantage of Player 
        | Deuce 
        | Game of Player

    /// Compute the next score in a game 
    let nextPointScore a = 
        match a with 
        | Zero -> Fifteen
        | Fifteen -> Thirty
        | Thirty -> Forty
        | Forty -> failwith "what??"

    /// Check if we've reached deuce
    let normalize score = 
        match score with 
        | Points(Forty,Forty) -> Deuce
        | _ -> score

    /// Score a point in a game
    let scorePoint score point =
        match score, point with 
        | Advantage player1, player2 when  player1 = player2 -> Game player1
        | Advantage player1, player2 -> Deuce
        | Deuce, player -> Advantage player
        | Points(Forty, _), A -> Game A
        | Points(_, Forty), B -> Game B
        | Points(a, b), A -> normalize (Points (nextPointScore a, b))
        | Points(a, b), B -> normalize (Points (a, nextPointScore b))
        | Game _ , _ -> (* printfn "the game is over!"; *) score

    /// Score a whole game, where the game is represented as a sequence of points
    let scoreGame (points: seq<Player>) = 
        Seq.scan scorePoint (Points(Zero,Zero)) points

    /// A sample game - A wins every point
    let game1 = seq { while true do yield A }

    /// A sample game - A and B swap points indefinitely
    let game2 = seq { while true do 
                         yield A 
                         yield B }

    /// A sample game - A and B trade points but A wins more points than B
    let game3 = seq { while true do 
                         yield A 
                         yield B 
                         yield A }

    scoreGame game1 |> Seq.truncate 10 |> Seq.toList


    scoreGame game2 |> Seq.truncate 10 |> Seq.toList

    scoreGame game3 |> Seq.truncate 10 |> Seq.toList


    /// Generate a random game
    let randomGame i = 
        let rnd = new System.Random(i) 
        seq { while true do if rnd.NextDouble() < 0.5 then yield A else yield B }

    // Random testing of 1000 games
    for i in 1 .. 1000 do 
        scoreGame (randomGame i)
           |> Seq.nth 10 
           |> printfn "result is %A"

