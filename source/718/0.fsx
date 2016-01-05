open System
open TescoApi.Tesco

// ------------------------------------------------------------------

type Code = string
type Name = string
type Price = decimal
type Picture = string
type Product = Product of Code * Name * Picture * Price
type Quantity = decimal

type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelLineItem of int

type LinePurchase = list<LineItem>
type FinalPurchase = Map<Product, Quantity>

// ------------------------------------------------------------------

let calculateFinal (line:LinePurchase) : FinalPurchase =
  line
  |> Seq.choose (fun item ->
      match item with
      | SaleLineItem(id, prod, q) -> 
          let cancelled =
            line |> Seq.exists (fun item ->
              match item with
              | CancelLineItem cancelId when cancelId = id -> true
              | _ -> false)
          if cancelled then None
          else Some (prod, q)
      | _ -> None )
  |> Seq.groupBy (fun (prod, q) -> prod)
  |> Seq.map (fun (prod, items) -> 
        prod, Seq.sumBy snd items)
  |> Map.ofSeq
