open System

let suits, ranks = "♣♦♥♠", "A123456789TJQK"
let deck = [|for s in suits do for r in ranks -> String[|r;s|]|]
let mutable index = 0
let shuffle () =
    let r = Random()
    let order = [|for _ in 1..deck.Length -> r.Next()|]
    Array.Sort(order,deck)
    index <- 0
do shuffle ()
let take() = let card = deck.[index] in index <- index + 1; card
let value card = 
    let values = 1::[1..9]@[0;0;0;0]
    let rank (card:string) = ranks.IndexOf(card.[0])
    values.[rank card]
let total cards = (cards |> List.sumBy value) % 10
let isNatural hand = let score = total hand in score = 8 || score = 9
type action = Hit | Stand
let playerRule total =
    match total with
    | 6 | 7 -> Stand
    | _ -> Hit
let bankerRule total thirdCardValue =
    let thirdCardIs (values:int list) =
        values |> List.exists ((=) thirdCardValue)
    match total with
    | 0 | 1 | 2 -> Hit
    | 3 when thirdCardIs [1;2;3;4;5;6;7;9;0] -> Hit
    | 4 when thirdCardIs [2;3;4;5;6;7] -> Hit
    | 5 when thirdCardIs [4;5;6;7] -> Hit
    | 6 when thirdCardIs [6;7] -> Hit
    | _ -> Stand
let applyRules player banker =
    if isNatural player || isNatural banker then player, banker
    else
        let draw = function Hit -> [take()] | Stand -> [] 
        let playerAction = playerRule (total player)
        let player = player@(draw playerAction)
        let bankerAction =
            if playerAction = Stand then playerRule (total banker)
            else
                let thirdCard = player.[2]
                bankerRule (total banker) (value thirdCard)    
        player, banker@(draw bankerAction)
let player, banker =
    let c1,c2,c3,c4 = take(), take(), take(), take()
    applyRules [c1;c3] [c2;c4]
let result = 
    let a, b = total player, total banker
    if a > b then sprintf "Player wins with %d to Banker's %d" a b
    elif a < b then sprintf "Banker wins with %d to Player's %d" b a
    else sprintf "Tie on %d" a
let game = 
    let cards xs = String.concat "," xs
    sprintf "Player %s, Banker %s\r\n%s" (cards player) (cards banker) result

module ``Baccarat Tests`` = 
    let ``card A♥ value is 1``() =
        assert(value "A♥" = 1)
    let ``card 6♦ value is 6``() =
        assert(value "6♦" = 6)
    let ``card K♠ value is 0``() =
        assert(value "K♠" = 0)
    let ``hand of A♣ & K♠ totals 1``() =
        assert(total ["A♣";"K♠"] = 1)
    let ``hand of 9♦ & 3♥ totals 2``() =
        assert(total ["9♦";"3♥"] = 2)
    let ``hand of K♥ & Q♣ totals 0``() =
        assert(total ["K♥";"Q♣"] = 0)
    let ``hand of 4♠ & 5♦ totals 9``() =
        assert(total ["4♠";"5♦"] = 9)
    let ``hand of 9♥ K♣ is natural``() =
        assert(isNatural ["9♥";"K♣"])
    let ``player stands on a total of 6``() =
        assert(playerRule 6 = Stand)        
    let ``player hits on a total of 5``() =
        assert(playerRule 5 = Hit)       
    let ``test banker rule``() =
        let H = Hit
        let S = Stand
        let table = [
            7,[S;S;S;S;S;S;S;S;S;S]
            6,[S;S;S;S;S;S;H;H;S;S]
            5,[S;S;S;S;H;H;H;H;S;S]
            4,[S;S;H;H;H;H;H;H;S;S]
            3,[H;H;H;H;H;H;H;H;S;H]
            2,[H;H;H;H;H;H;H;H;H;H]
            1,[H;H;H;H;H;H;H;H;H;H]
            0,[H;H;H;H;H;H;H;H;H;H]
            ]
        for total, thirdCards in table do
            thirdCards |> List.iteri (fun i action ->
                assert(action = bankerRule total i)   
            )

    do  ``card A♥ value is 1``()
        ``card 6♦ value is 6``()
        ``card K♠ value is 0``()
        ``hand of A♣ & K♠ totals 1``()
        ``hand of 9♦ & 3♥ totals 2``()
        ``hand of K♥ & Q♣ totals 0``()
        ``hand of 9♥ K♣ is natural``()
        ``player stands on a total of 6``()
        ``player hits on a total of 5``()
        ``test banker rule``()