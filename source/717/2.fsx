// [snippet:Representation of the checkout domain model]
// Type aliases to make domain code readable
type Code = string
type Price = decimal
type Quantity = decimal
type Amount = decimal
type Name = string

/// For every product, we store code, name and price
type Product = Product of Code * Name * Price

/// Different options of payment
type TenderType = 
  | Cash
  | Card
  | Voucher

/// Represents scanned entries at checkout
type LineItem = 
  | Sale of Product * Quantity
  | Cancel of int
  | Tender of Amount * TenderType
// [/snippet]

// [snippet:Database of products and lookup]
let products =
  [ Product("50082728", "Lynx Africa", 0.99M);
    Product("9781933988924", "Real World FP", 29.99M) ]

/// Lookup product in the 'products' list
let lookup query = 
  products |> Seq.tryFind (fun (Product(code, _, _)) ->
    code = query)

/// Calculate the tototal price for scanned items
/// (Cancellation is not supported yet)
let calculateTotal (items:seq<LineItem>) =
  items |> Seq.sumBy (fun item ->
    match item with
    | Sale(Product(_, _, price), quantity) ->
        price * quantity
    | Cancel n -> 
        failwith "Not implemented"
    | Tender _ -> 0.0M )
// [/snippet]

// [snippet:Console-based user interface]
open System

/// Active pattern that succeeds if product
/// with the specified code exists (and returns it)
let (|LookupProduct|_|) code = lookup code

/// Active pattern that succeeds if code 
/// represents cancellation ("C<index>")
let (|CancelCode|_|) (code:string) = 
  if code.StartsWith("C") then Some(int(code.Substring(1)))
  else None

/// The main program loop
let main() = 
  let items = new ResizeArray<LineItem>()
  let mutable finished = false
  while not finished do
    Console.Write("> ")
    match Console.ReadLine() with
    | null
    | "" -> 
        let total = calculateTotal items
        printfn "TOTAL: %A" total
        finished <- true
    | CancelCode id ->
        printfn "Cancel: %d" id
        items.Add(Cancel(id))
    | LookupProduct prod ->
        items.Add(Sale(prod, 1.0M))
        let (Product(_, name, price)) = prod
        printfn "Added: %s (%A)" name price
    | _ -> 
        printfn "Unknown product"
      
printfn "WELCOME TO TESCO"
main()
// [/snippet]