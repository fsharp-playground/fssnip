

type OptionKind = Call | Put 

type Option = 
  | European of OptionKind * float
  | Combine of Option * Option
  | Times of int * Option
  static member (*) (k:int, option) =
    Times(k, option)
  static member (*) (option, k:int) =
    Times(k, option)
  static member (+) (option1, option2) =
    Combine(option1, option2)
  static member (-) (option1, option2) =
    Combine(option1, Times(-1, option2))

let rec payoff price option =
  match option with
  | European(Call, exercise) ->
      max (price - exercise) 0.0
  | European(Put, exercise) ->
      max (exercise - price) 0.0
  | Times(n, option) ->
      (float n) * (payoff price option)
  | Combine(option1, option2) ->
      (payoff price option1) + (payoff price option2) 

let printTrades option = 
  let rec loop buyOrSell option =
    for i in 0 .. 5 do printfn "TraDE!!!"
  loop true option
    
printTrades (ButterflySpread 20.0 40.0)


let BottomStraddle =
  Combine
    ( European(Call, 30.0),
      European(Put, 30.0) )

let ($) option1 option2 = 
  Combine(option1, option2)

let Sell(option) = Times(-1, option)

let ButterflySpread low high =
  let mid = (low + high) / 2.0
  European(Call, low) -
  3 * European(Call, mid) +
  European(Call, high)

// Buy call at 20.0
// Sell call at 30.0
// Sell call at 30.0
// Sell call at 30.0
// Buy call at 40.0

European(Call, 10.0)
// Buy call at 10.0
Times(-1, European(Call, 10.0))
// Sell call at 10.0
Times(-1, Times(-1, European(Call, 10.0)))
// Buy call at 10.0



#load "lib/FSharpChart.fsx"
open Samples.FSharp.Charting
let optionPayoff option = 
  [ for p in 0.0 .. 10.0 .. 100.0 -> p, option p ]

optionPayoff (fun actual ->
  payoff actual BottomStraddle) |> Chart.Line
