let  range     = [0..999]
let  sumFunc   = List.fold (+) 0
let  condition = fun  x ->  x % 3 = 0 || x % 5 = 0
List.filter condition range |> sumFunc |> System.Console.Write