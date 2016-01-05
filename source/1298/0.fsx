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

type Product = Code * Name * UnitPrice

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

let products =
  [ "A1", "Border oat crumbles", 0.69M<GBP/Q>
    "B1", "Tea", 1.49M<GBP/Q>
    "C1", "Phil's phone", 299.9M<GBP/Q>
    "D1", "Phil's mac", 1200.0M<GBP/Q> ]

let lookup (search:Code) : Product option = 
  products
  |> List.tryFind (fun (code, _, _) -> code = search)

// ----------------------------------------------
// Calculations
// ----------------------------------------------

open System

let total (basket:Basket) =
  basket |> List.sumBy (fun item ->
    match item with
    | SaleItem((_, _, price), q) -> 
        price * q
    | TenderItem(_, value) -> 0.0M<GBP>
    | CancelItem(index) ->
        let cancelled = List.nth basket index
        match cancelled with
        | SaleItem((_, _, price), q) ->
            -1.0M * price * q
        | CancelItem _ | TenderItem _ -> 
            invalidOp "You can only cancel SaleItems!" )
        
let rec purchase (basket:Basket) =
  let code = Console.ReadLine()
  match code, lookup code with
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

let basket = purchase []
printfn "Purchase: %A" basket
printfn "Total: %A" (total basket)