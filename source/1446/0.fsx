#load "Setup.fsx"
open Setup

// ============================================================================
// PART 2: Creating DSL for financial contracts
// ============================================================================

/// Option can be either Call or Put option
type OptionType =
    | Call 
    | Put 

/// Represents information about European option
type OptionInfo = 
    { Identifier : string
      ExercisePrice : float
      TimeToExpiry : float 
      Kind : OptionType }

/// Contracts are composed from options
type Option = 
  | European of OptionInfo
  | Combine of Option * Option
  | Times of int * Option
  static member (*) (x:int, option:Option) =
    Times(x, option)     
  static member ($) (option1:Option, option2:Option) = 
    Combine(option1, option2)  

// ----------------------------------------------------------------------------
// DEMO: Creating simple options & extending the syntax
// ----------------------------------------------------------------------------

// Define helper function that make the syntax nicer
let Sell (option:Option) = -1 * option

let EuroPut id price tte = 
  { Identifier = id; TimeToExpiry = tte
    ExercisePrice = price; Kind = Put }
  |> European  

let EuroCall id price tte = 
  { Identifier = id; TimeToExpiry = tte
    ExercisePrice = price; Kind = Call }
  |> European  

Sell(EuroPut "MSFT" 30.0 10.0) $
EuroCall "MSFT" 30.0 10.0


/// Function that builds the butterfly spread contract
let ButterflySpread name lowPrice highPrice = 
  (EuroCall name lowPrice 1.0) $
  (EuroCall name highPrice 1.0) $
  Sell(2 * (EuroCall name ((lowPrice + highPrice) / 2.0) 1.0))

/// Function that builds the bottom straddle contract
/// Call at exercisePrice + Put at exercisePrice
let BottomStraddle name exercisePrice = 
  (EuroCall name exercisePrice 1.0) $
  (EuroPut name exercisePrice 1.0)
  
// ============================================================================
// PART 2: Processing the contracts
// ============================================================================

#load "../packages/FSharp.Charting.0.90.6/FSharp.Charting.fsx"
#load "Setup.fsx"
open FSharp.Charting
open Setup

/// Calculating the actual actualPayoff based on current price
let rec actualPayoff option = 
  match option with
  | European(info) ->
      let actualPrice = getLatestPrice info.Identifier
      // TODO: Calculate the payoff
      match info.Kind with 
      | Call -> __
      | Put -> __
  | Combine(left, right) ->
      (actualPayoff left) + (actualPayoff right)
  // TODO: Add the case for 'Times(r, option)'

actualPayoff (ButterflySpread "MSFT" 30.0 60.0)
actualPayoff (ButterflySpread "MSFT" 20.0 50.0)
actualPayoff (ButterflySpread "MSFT" 0.0 70.0)

// ----------------------------------------------------------------------------
// DEMO: Generalizing the evaluation function
// ----------------------------------------------------------------------------

/// Calculating the actual payoff based on current price
let rec eval pricing = function
  | European(info) ->
      let actualPrice = pricing info
      match info.Kind with 
      | Call -> max (actualPrice - info.ExercisePrice) 0.0
      | Put -> max (info.ExercisePrice - actualPrice) 0.0
  | Combine(left, right) ->
      (eval pricing left) + (eval pricing right)
  | Times(r, option) ->
      float r * (eval pricing option)

// ----------------------------------------------------------------------------
// DEMO: Simulating prices using Monte-carlo
// ----------------------------------------------------------------------------

open System
open MathNet.Numerics.Distributions

// Functions from Lecture 3

let simulatePriceSeries drift volatility dt initial (dist:Normal) = 
  let driftExp = (drift - 0.5 * pown volatility 2) * dt
  let randExp = volatility * (sqrt dt)
  let rec loop price = seq {
    yield price
    let price = price * exp (driftExp + randExp * dist.Sample()) 
    yield! loop price }
  loop initial

let simulatePrice (tte:float) (info:StockInfo) = 
  let dist = Normal(0.0, 1.0)
  simulatePriceSeries info.Drift info.Volatility 0.005 info.CurrentPrice dist
  |> Seq.skip (int (tte * 252.0))
  |> Seq.head

let info = getStockInfo "MSFT" (DateTime(2012,1,1)) (DateTime(2013,1,1))
simulatePrice 1.0 info


// Calling simulatePrice on current data from MSFT

let simulatedPricing info =
  let stats = getStockInfo info.Identifier (DateTime(2012,1,1)) (DateTime(2013,1,1))
  simulatePrice info.TimeToExpiry stats

eval simulatedPricing (ButterflySpread "MSFT" 30.0 60.0)
eval simulatedPricing (ButterflySpread "MSFT" 20.0 50.0)
eval simulatedPricing (ButterflySpread "MSFT" 0.0 70.0)


// Performing a simple Monte-carlo calculation

let mc1 = Array.init 5000 (fun _ -> ButterflySpread "MSFT" 20.0 50.0)
let mc2 = __ // TODO: BottomStraddle

let payoffs1 = mc2 |> Array.map (eval simulatedPricing)
let payoffs2 = __

payoffs2 
|> Seq.countBy (fun v -> round v)
|> Chart.Column


// ============================================================================
// PART 2: TASKS
// ============================================================================

// TASK #1: Rebuild the Strangle and BullSpread contracts
// (from Part1) using the domain specific language that
// we defined in this section.


// TASK #2: Write the 'payoff' function using the general
// 'eval' function defined in the next step. Use it to evaluate
// the actual payoff (based on current MSFT stock prices) for
// a sample of Strange & BullSpread contracts.


// TASK #3: Calculate margin - margin is calculated by taking 
// 1% of every stock traded as part of the contract. So, for 
// example, if you have:
//
//  (EuroCall "MSFT" 30.0 1.0) $
//  (Sell (EuroPut "MSFT" 30.0 1.0))
//
// Then the expected result should be 2 times "1% of current MSFT price"
// (the fact that we are buying one and selling another has no effect
// in this case, so you cannot use the general 'eval' function!)


// TASK #4: In the current model, the "Combine" option represents a case
// when we always execute both legs of the contract (e.g. we buy/sell both
// the left and right leg). Add an option "Choice" that represents the 
// situation where we can choose either the left or the right leg (depending
// on what is better for us) and update the 'actualPayoff' and 'eval' functions
// to handle this new case.


// TASK #5 (BONUS): Go back to the Black-Scholes equation that we implemented
// in lecture 3 and use it to evaluate the price of the composed
// contract - to do this, you can follow similar pattern as in the 
// MonteCarlo simulation, but use Black-Scholes to calculate the price
// in the 'pricing' function used as an argument to 'eval'.
