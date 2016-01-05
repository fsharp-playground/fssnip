module Starbucks

type size = Tall | Grande | Venti
type drink = Latte | Cappuccino | Mocha | Americano
type extra = Shot | Syrup

type Cup = { Size:size; Drink:drink; Extras:extra list } with
    static member (+) (cup:Cup,extra:extra) =
        { cup with Extras = extra :: cup.Extras }
    static member Of size drink =
        { Size=size; Drink=drink; Extras=[] }
    
let Price (cup:Cup) =
    let tall, grande, venti = 
        match cup.Drink with
        | Latte      -> 2.69, 3.19, 3.49
        | Cappuccino -> 2.69, 3.19, 3.49
        | Mocha      -> 2.99, 3.49, 3.79
        | Americano  -> 1.89, 2.19, 2.59
    let basePrice =
        match cup.Size with
        | Tall -> tall 
        | Grande -> grande
        | Venti -> venti
    let extras =
        cup.Extras |> List.sumBy (function
            | Shot -> 0.59
            | Syrup -> 0.39
        )
    basePrice + extras

let myCup = Cup.Of Venti Latte + Syrup
let price = Price myCup