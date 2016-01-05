// Cartesian product of a sequence of sequences.
let rec cartSeq (nss:seq<#seq<'a>>) = 
  let f0 (n:'a) (nss:seq<#seq<'a>>) = 
    match Seq.isEmpty nss with
    | true -> Seq.singleton(Seq.singleton n)
    | _ -> Seq.map (fun (nl:#seq<'a>)->seq{yield n; yield! nl}) nss
  match Seq.isEmpty nss with
  | true -> Seq.empty
  | _ -> Seq.collect (fun n->f0 n (cartSeq (Seq.skip 1 nss))) (Seq.head nss)
 
 
// Test.

let choices = 
  [
    ["crispy";"thick";"deep-dish";];
    ["pepperoni";"sausage";];
    ["onions";"peppers";];
    ["mozzarella";"provolone";"parmesan"];
  ] 
 
let pizzas = cartSeq choices

pizzas |> Seq.iter (printfn "%A")