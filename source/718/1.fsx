// [snippet:Types representing Tesco products]
type Code = string
type Name = string
type Price = decimal
type Picture = string
type Product = Product of Code * Name * Picture * Price
type Quantity = decimal
// [/snippet]

// [snippet:Representation using scanned items]
type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelLineItem of int

/// Purchase is a list of items as they were scanned
type LinePurchase = list<LineItem>
// [/snippet]

// [snippet:Representation for the final bill]
/// Immutable map stores products and the purchased quantity
type FinalPurchase = Map<Product, Quantity>
// [/snippet]

// [snippet:Finalizing the purchase]
/// This function takes a list of scanned lines and
/// produces a final bill. It first removes all cancelled
/// items and then groups products to get total quantity.
let calculateFinal (line:LinePurchase) : FinalPurchase =
  line
  |> Seq.choose (fun item ->
      match item with
      | SaleLineItem(id, prod, q) -> 
          // Check if the item has been cancelled
          let cancelled =
            line |> Seq.exists (fun item ->
              match item with
              | CancelLineItem cancelId when cancelId = id -> true
              | _ -> false)
          if cancelled then None
          else Some (prod, q)
      | _ -> None )

  // Group products and calculate total quantity
  |> Seq.groupBy (fun (prod, q) -> prod)
  |> Seq.map (fun (prod, items) -> 
        prod, Seq.sumBy snd items)
  |> Map.ofSeq
// [/snippet]