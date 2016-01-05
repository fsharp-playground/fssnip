// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

type Score = 
    | Love
    | Five
    | Thirty
    | Forty
    | Adv
    | Win
    with 
        member x.Won() = 
            match x with
            | Love -> Five
            | Five -> Thirty
            | Thirty -> Forty
            | Forty -> Win
            | Adv -> Win
            | Win -> Win
        member x.Lost() =
            match x with
            | Adv -> Forty
            | _ -> x

type Player = A | B

type GameState =
    | InPlay of Score * Score
    | Won of string
           

let succeed x = x
let fail = None
let bind p rest =
    printfn "%A" p
    match p with
    | InPlay(Score.Win, _) -> Won("Player 1")
    | InPlay(_, Score.Win) -> Won("Player 2")
    | _ -> rest p

let delay f = f()

type GameBuilder() =
    member b.Return(x)  = succeed x
    member b.Bind(p, rest) = bind p rest
    member b.Delay(f)   = delay f
    member b.Let(p,rest) : GameState = rest p

let game = GameBuilder()


let play player state =
     match state with
     | InPlay(p1,p2) -> 
           match player with
           | A when p1 = Forty && p2 = Forty -> InPlay(Adv, p2.Lost())
           | B when p2 = Forty && p2 = Forty -> InPlay(p1.Lost(), Adv) 
           | A -> InPlay(p1.Won(), p2.Lost())
           | B -> InPlay(p1.Lost(), p2.Won())
     | _ -> state

let start = InPlay(Love,Love)

let result = game { 
                    let! a = play A start
                    let! b = play B a
                    let! c = play A b
                    let! d = play B c
                    let! e = play A d
                    let! f = play B e
                    let! g = play A f
                    let! h = play B g
                    let! i = play B h
                    return i
                 }