namespace Retail

// ----------------------------------------------
// Domain
// ----------------------------------------------
type [<Measure>] GBP
type [<Measure>] Q

type UnitPrice = decimal<GBP/Q>
type Amount = decimal<GBP>
type Name = string
type Code = string
type Quantity = decimal<Q>

type Product = 
  { Code : Code
    Name : Name 
    Price : UnitPrice }

type Tender = 
  | Cash 
  | Card of string 
  | Voucher of Code 

type LineItem =
  | SaleItem of Product * Quantity
  | TenderItem of Tender * Amount
  | CancelItem of int

type Basket = list<LineItem>

// ----------------------------------------------
// Data
// ----------------------------------------------
module Data = 
  open FSharp.Data

  type Products =
    JsonProvider<"""[
      { "code":"A1", "name":"Border oat crumbles", "price":0.69 },
      { "code":"B1", "name":"Tea", "price":1.49 },
      { "code":"C1", "name":"Phil's phone", "price":299.9 },
      { "code":"D1", "name":"Phil's mac", "price":1200.0 } ]""">

  let products =
    [ for it in Products.GetSample() ->
        { Code = it.Code; Name = it.Name
          Price = it.Price * 1.0M<GBP/Q> } ]

  let lookup (search:Code) : Product option = 
    products
    |> List.tryFind (fun p -> p.Code = search)

// ----------------------------------------------
// Calculations
// ----------------------------------------------

module Calculation = 
  open System

  let total (basket:Basket) =
    basket |> List.sumBy (fun item ->
      match item with
      | SaleItem(prod, q) -> 
          prod.Price * q
      | TenderItem(_, value) -> 0.0M<GBP>
      | CancelItem(index) ->
          let cancelled = List.nth basket index
          match cancelled with
          | SaleItem(prod, q) ->
              -1.0M * prod.Price * q
          | CancelItem _ | TenderItem _ -> 
              invalidOp "You can only cancel SaleItems!" )
        
  let rec purchase (basket:Basket) =
    let code = Console.ReadLine()
    match code, Data.lookup code with
    | "q", _ ->
        printfn "Finished"
        basket
    | _, None -> 
        printfn "Not found: %s" code
        purchase basket
    | _, Some prod ->
        printfn "Adding: %A" prod
        let item = SaleItem(prod, 1.0M<Q>)
        purchase (item::basket)

// ----------------------------------------------
// "User interface"
// ----------------------------------------------
module Main = 
  open Calculation

  let basket = purchase []
  printfn "Purchase: %A" basket
  printfn "Total: %A" (total basket)