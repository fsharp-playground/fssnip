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


let player1Point state =
    match state with
    | InPlay(p1,p2) -> InPlay(p1.Won(),p2.Lost())
    | _ -> state

let player2Point state =
    match state with
    | InPlay(p1,p2) -> InPlay(p1.Lost(),p2.Won())
    | _ -> state

let start = InPlay(Love,Love)

let result = game { 
                    let! a = player1Point start
                    let! b = player1Point a
                    let! c = player1Point b
                    let! d = player1Point c
                    return d
                 }