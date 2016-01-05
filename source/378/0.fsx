
open Microsoft.FSharp.Reflection

type Score = | Love | Fifteen | Thirty | Forty | Deuce | Advantage | Disadvantage | Win | Lose

type Player = {name : string; score : Score}

/// Returns the name of a union case as a string
let GetUnionCaseName(x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name  

/// Formats a single score - eg. Score.Fifteen -> "Fifteen"
let formatScore =
    function
        | Win -> "Game"
        | Lose -> "Loses"
        | other -> GetUnionCaseName other
    
/// Formats the scores for an array of two players
let formatScores (players : array<Player>) =
    match players with
    | [|p1; _|] when p1.score = Deuce -> formatScore p1.score
    | [|p1; _|] when p1.score = Advantage -> formatScore p1.score + " "  + p1.name
    | [|_; p2|] when p2.score = Advantage -> formatScore p2.score + " "  + p2.name
    | [|p1; _|] when p1.score = Win -> formatScore p1.score + " " + p1.name
    | [|_; p2|] when p2.score = Win -> formatScore p2.score + " " + p2.name
    | [|p1; p2|] when p1.score = p2.score -> formatScore p1.score + " All"
    | [|p1; p2|] -> formatScore p1.score + " " + formatScore p2.score
    | _ -> failwith "Impossible combination"

/// Maps a current score into a new score
let nextScore (winnerScore, loserScore) =
    let score =
        match (winnerScore, loserScore) with
        | (Love, any) -> (Fifteen, any)
        | (Fifteen, any) -> (Thirty, any)
        | (Thirty, any) when any < Forty -> (Forty, any)
        | (Thirty, any) when any = Forty -> (Deuce, Deuce)
        | (Forty, any) when any < Forty -> (Win, Lose)
        | (Forty, any) when any = Forty -> (Advantage, Disadvantage)
        | (Deuce, Deuce) -> (Advantage, Disadvantage)
        | (Disadvantage, Advantage) -> (Deuce, Deuce)
        | (Advantage, Disadvantage) -> (Win, Lose)
        | _ -> failwith ("Impossible combination: " + formatScore winnerScore + "/" + formatScore loserScore)
    [|fst(score); snd(score)|]

/// Returns a new 2-element player array on the assumption that player.[winnerIndex] has won a point
let winPoint (players : array<Player>, winnerIndex : int) =
    let loserIndex = (winnerIndex + 1) % 2
    let winner, loser = players.[winnerIndex], players.[loserIndex]
    let newScores = nextScore (winner.score, loser.score)
    // Using winner/loserIndex in this way ensures the resulting scores end up in the right slots:
    [|{players.[0] with score=newScores.[winnerIndex]}; {players.[1] with score=newScores.[loserIndex]}|]

/// Takes an array of players and array of winner indexes and summarises the progress of points
let play players runOfPlay =
    runOfPlay
    |> Array.fold (fun score winnerIndex -> 
                        let newScore = winPoint(score, winnerIndex)
                        printfn "%s" (formatScores newScore)
                        newScore) players 
  
// Test:

let players = [|{name="Sharapova"; score=Love}; {name="Kvitova"; score=Love}|]

let walkover = [|0;0;0;0|]
play players walkover |> ignore
   
let notTooHard = [|0;0;1;1;0;1;1;1|]
play players notTooHard |> ignore

let hardFought = [|0;1;0;1;0;1;1;0;1;0;1;0;0;0|]
play players hardFought |> ignore
