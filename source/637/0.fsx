open System

// Retail domain on a single page

type Name = string
type Barcode = string
type Price = decimal
type Quantity = decimal

type Product = Product of Name * Barcode * Price
type LineItem =
  | SaleLineItem of int * Product * Quantity
  | CancelLine of int 

type Basket private (items) = 
  member basket.AddItem(lineId, product, quantity) =
    let item = SaleLineItem(lineId, product, quantity)
    Basket(item::items)
  member basket.CancelLine(id) = 
    let item = CancelLine(id)
    Basket(item::items)
  member basket.Total = 
    items 
      |> List.map (fun line ->
        match line with
        | SaleLineItem(_, Product(_, _, price), quantity) ->
            price * quantity 
        | CancelLine id -> 
            items |> List.pick (fun line ->
              match line with
              | SaleLineItem(lineId, Product(_, _, price), quantity) 
                    when lineId = id -> 
                  Some(-1.0M * price * quantity)
              | _ -> None ) )    
      |> List.sum
  new() = Basket([])

// Product list and lookup

let products = 
  [ Product("Tea", "070177075101", 1.50M)
    Product("Real-World Functional Programming", "9781933988924", 49.99M) ]

let lookup searchCode = 
  products |> List.tryFind (fun (Product(_, code, _)) -> 
    code = searchCode)

// Main loop

let rec readItems lineId (basket:Basket) =
  printfn "Enter item %d: " lineId
  let search = Console.ReadLine()
  if String.IsNullOrEmpty search then 
    printfn "Total: %A" basket.Total
  elif search.StartsWith("-") then
    let id = -1 * (int search)
    readItems lineId (basket.CancelLine(id))
  else
    let prod = lookup search
    match prod with
    | Some(prod) -> 
        readItems (lineId + 1) (basket.AddItem(lineId, prod, 1.0M))
    | _ -> 
        readItems lineId basket 

readItems 0 (Basket())