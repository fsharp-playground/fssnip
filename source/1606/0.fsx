// ------------------------------------------------------------------
// Domain.fs
// ------------------------------------------------------------------

namespace Tesco.Domain

type BarCode = string
type Name = string
type Price = decimal
type Quantity = decimal

type Product = 
  { Name : Name
    BarCode : BarCode
    Price : Price }

type ProductList = list<Product>

type TenderType = 
  | Cash 
  | Card 
  | Voucher 

type SaleLine = 
  | ScanLine of Product * Quantity
  | CancelLine of int
  | TenderLine of TenderType * Price

type Sale = list<SaleLine>

// ------------------------------------------------------------------
// Main.fs
// ------------------------------------------------------------------

module Tesco.Main
open Tesco.Domain

let database =
  [ { Name="Hand sanitizer"; BarCode="5055028300057"; Price=3.99M }
    { Name="Record cards"; BarCode="5014108161018"; Price=2.99M }
    { Name="PostIt Notes"; BarCode="051135813317"; Price=1.99M } ]

let findProduct barcode = 
  database |> List.tryFind (fun prod ->
    prod.BarCode = barcode)

open System

let totalPrice (sale:Sale) = 
  sale |> List.sumBy(fun line ->
    match line with
    | ScanLine(prod, quant) -> prod.Price * quant
    | CancelLine(index) ->
        match sale.[index] with
        | ScanLine(prod, quant) -> -(prod.Price * quant)
        | _ -> failwith "Can only cancel scan lines"
    | TenderLine(_, _) -> 0.0M)        

let rec readSale (sale:Sale) =
  let code = Console.ReadLine()
  if code = "q" then
    List.rev sale
  else
    match findProduct code with
    | Some prod -> 
        printfn "Adding: %s" prod.Name
        let line = ScanLine(prod, 1.0M)
        let sale = line::sale
        readSale sale
    | None ->
        printfn "No candy :-("
        readSale sale

[<EntryPoint>]
let main argv = 
    let sale = readSale []
    let total = totalPrice sale
    printfn "Your purchase:\n%A" sale
    printfn "\nTotal: %A" total
    0 // return an integer exit code
