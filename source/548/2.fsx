// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

type Player = A | B

type Score = 
    | Love
    | Five
    | Thirty
    | Forty

type GameState = 
    | InPlay of Score * Score
    | Duece
    | Advantage of Player
    | Won of string

type GameFunc = (GameState -> GameState)

let delay f = f()

let start = InPlay(Love,Love)

let play player state =
     let nextScore score =
         match score with
         | Love -> Five
         | Five -> Thirty
         | Thirty -> Forty
         | Forty -> Forty

     match state with
     | InPlay(p1,p2) -> 
          match player with 
          | _ when (p1 = Thirty && p2 = Forty) || (p1 = Forty && p2 = Thirty) -> Duece
          | A when p1 = Forty -> Won("Player 1")
          | B when p2 = Forty -> Won("Player 2")
          | A -> InPlay(nextScore p1, p2)
          | B -> InPlay(p1, nextScore p2)
     | Duece -> Advantage(player)
     | Advantage(p) when p = player ->  Won("Player 1")
     | Advantage(_) -> Duece  
     | _ -> state

type GameBuilder() =
    member b.Zero() = (fun _ -> start)
    member b.Yield(a : GameFunc) = a
    member b.Combine(a : GameFunc, b' : GameFunc) = a >> b'
    member b.For(vals : seq<Player>, f : Player -> GameFunc) = 
         vals |> Seq.fold (fun s n -> b.Combine(s, f n)) (b.Zero())
 
    static member Start (state : GameState) (func : GameFunc) = 
        func(state)
    
let game = GameBuilder()

let playerThatWins = [A;B;A;B;A;B;B;A;A;A] |> Seq.ofList

let result = game {
                    for playerwin in playerThatWins do 
                        yield play playerwin 
             } |> GameBuilder.Start start