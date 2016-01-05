open System

// --------------------------------------------------------
// Get price on a specified day 

let prices = Yahoo.GetPriceTable(DateTime(2011, 1, 1))
let p1 = prices.["MSFT", DateTime(2011, 1, 10)]
let p2 = prices.["YHOO", DateTime(2012, 1, 10)]

// Print prices using 'printfn' function:
printfn "%f %f" p1 p2

// --------------------------------------------------------
// Simple representation of financial contracts

type Amount = float
type Ticker = string

type Contract = 
  | Trade of Ticker * Amount
  | After of DateTime * Contract
  | Until of DateTime * Contract
  | Both of Contract * Contract

// --------------------------------------------------------
// Sample trades and pattern matching

// Create several values representing contracts
let msft = Trade("MSFT", 1000.0)
let aapl = Trade("AAPL", 200.0)
let both = Both(msft, aapl)

// Print name of a trade, if it is simple Trade
match msft with
| Trade(name, amount) -> printfn "Trade: %s" name
| _ -> printfn "Complex trade"

// --------------------------------------------------------
// TASK #1: Write a function that takes a trade and prints 
// its value (if the trade is Trade) using 'prices' above

// (...)
//
// We would like to be write:
//   evaluate msft (DateTime(2011, 1, 10))

// --------------------------------------------------------
// Simplify construction using functions and operator

let trade (what, amount) = Trade(what, amount)
let after dt contract = After(dt, contract)
let until dt contract = Until(dt, contract)
let ($) c1 c2 = Both(c1, c2)

// TASK #2a: Write function 'sell' that behaves like
// 'trade', but makes the amount negative
// TASK #2b: Write function 'onDate' that creates a 
// trade that can happen only on a specified day

// (...)
 
let itstocks = 
  after (DateTime(2012, 4, 1)) (trade ("MSFT", 1000.0)) $
  onDate (DateTime(2012, 5, 15)) (sell ("AAPL", 200.0))

// --------------------------------------------------------
// Recursive processing of contracts - print what 
// trades can happen on a specified day

let rec run contract day = 
  match contract with 
  | Trade(what, amount) -> 
      printfn "%s (%f)" what amount
  | After(dt, contract) ->
      if day >= dt then run contract day
  | Until(dt, contract) ->
      if day <= dt then run contract day
  | Both(contract1, contract2) ->
      run contract1 day
      run contract2 day

// Test the function using IT stocks
run itstocks (DateTime(2012, 3, 1))
run itstocks (DateTime(2012, 5, 10))

// --------------------------------------------------------
// TASK #3: Write a function 'evaluate' that recursively
// processes the contract and evaluates the total price
// on a specified day using the 'prices' table

// TASK #4 (BONUS): Extend the 'Contract' type and add a 
// new case 'Choice' that represents a choice between two
// contracts. Update the 'evaluate' function to return 
// both minimal and maximal price (depending on which 
// branch of 'Choice' is executed.)

// TASK #5 (BONUS): Write a function 'opposite' that takes
// a contract and builds a new contract where all amounts
// of 'Trade' elements are negative