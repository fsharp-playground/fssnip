let add5 x = x + 5 
let add5ToAll = List.map add5

[1..20] |> add5ToAll