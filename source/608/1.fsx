open System

let suits, ranks = "♣♦♥♠", "A123456789TJQK"
let deck = [|for s in suits do for r in ranks -> String[|r;s|]|]
do  let r = Random()
    let order = [|for _ in 1..deck.Length -> r.Next()|]
    Array.Sort(order,deck)
let mutable index = 0
let take() = let card = deck.[index] in index <- index + 1; card
let rank (card:string) = ranks.IndexOf(card.[0])
let values = 1::[1..9]@[0;0;0;0]
let value card = values.[rank card]
let total cards = (cards |> List.sumBy value) % 10
let isNatural hand = let score = total hand in score = 8 || score = 9
let playerRule hand =
    match total hand with
    | 6 | 7 -> hand
    | _ -> hand@[take()]
let bankerRule hand playerThirdCard =
    let cardValue = value playerThirdCard
    let thirdCardIs (values:int list) =
        values |> List.exists ((=) cardValue)
    let draw () = hand@[take()]
    match total hand with
    | 0 | 1 | 2 -> draw()
    | 3 when thirdCardIs [1;2;3;4;5;6;7;9;0] -> draw()
    | 4 when thirdCardIs [2;3;4;5;6;7] -> draw()
    | 5 when thirdCardIs [4;5;6;7] -> draw()
    | 6 when thirdCardIs [6;7] -> draw()
    | _ -> hand
let applyRules player banker =
    if isNatural player || isNatural banker then player, banker
    else 
        let player = playerRule player
        player,
            if player.Length = 2 then playerRule banker
            else bankerRule banker player.[2]
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