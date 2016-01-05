// Cartesian product of a list of lists.
let rec cartList nll = 
  let f0 n nll =  
    match nll with
    | [] -> [[n]]
    | _ -> List.map (fun nl->n::nl) nll
  match nll with
  | [] -> []
  | h::t -> List.collect (fun n->f0 n (cartList t)) h
 
 
// Test.

let choices = 
  [
    ["crispy";"thick";"deep-dish";];
    ["pepperoni";"sausage";];
    ["onions";"peppers";];
    ["mozzarella";"provolone";"parmesan"];
  ] 
 
let pizzas = cartList choices

pizzas |> Seq.iter (printfn "%A")
