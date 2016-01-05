let isEven n = n%2 = 0
let formatInt n = (string n) + "N"

// [snippet:Filtering and projection]
let res = 
  [ 1 .. 10 ] 
  |> List.filter isEven
  |> List.map formatInt
// [/snippet]