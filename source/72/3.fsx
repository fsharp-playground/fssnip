// [snippet:Using declared functions]
let range = [0..999]
let condition x = x % 3 = 0 || x % 5 = 0

range 
  |> List.filter condition 
  |> List.fold (+) 0
// [/snippet]
// [snippet: Using lambda functions]
[0..999] 
  |> Seq.filter (fun x -> x % 3 = 0 || x % 5 = 0) 
  |> Seq.reduce (+)
// [/snippet]
// [snippet: Using sequence expressions]
seq { for x in 0 .. 999 do
      if x % 3 = 0 || x % 5 = 0 then yield x }
|> Seq.sum
// [/snippet]
// [snippet: Using pattern match for ultimate speed]
let rec multiple x =
  match x with
  | _ when x = 1000 -> 0
  | _ when x%3 = 0 || x%5 = 0 -> x + multiple (x+1)
  | _ -> multiple (x+1)
  
multiple 1
// [/snippet]