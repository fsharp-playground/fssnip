// [snippet:Implementation]
open System

/// Defines how a contract can be constructed
type Contract = 
  | Trade of int * string
  | After of DateTime * Contract
  | Until of DateTime * Contract
  | Combine of Contract * Contract


/// Evaluate contract on a specific day
let rec eval contract (day:DateTime) = 
  [ match contract with
    | Trade(a, n) -> yield (a, n) 
    | Combine(c1, c2) -> 
        yield! eval c1 day
        yield! eval c2 day
    | After(dt, c) when day > dt -> yield! eval c day
    | Until(dt, c) when day < dt -> yield! eval c day
    | _ -> () ]


// Functions for creating contracts
let trade (amount, what) = Trade (amount, what)
let after dt contract = After (dt, contract)
let until dt contract = Until (dt, contract)
let ($) c1 c2 = Combine(c1, c2)
// [/snippet]

// [snippet:Examples of a trade description]
let msft = trade (500, "MSFT")
let goog = trade (100, "GOOG")

// What trades may happen today?
eval msft DateTime.Now
eval goog DateTime.Now

// Compose more complex contracts and evaluate them
let itstocks =   
  after (DateTime(2009, 4, 15)) msft $
  after 
    (DateTime(2009, 4, 10)) 
    (until (DateTime(2009, 4, 20)) goog)

eval itstocks DateTime.Now
eval itstocks (DateTime(2009, 4, 12))
eval itstocks (DateTime(2009, 4, 18))
// [/snippet]