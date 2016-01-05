module Option = 
  let map2 f a b =
     a |> Option.bind (fun a' -> b |> Option.map (fun b' -> f a' b'))

//test
Option.map2 (+) (Some "a") None
Option.map2 (+) (Some "a") (Some "b")

Option.map2 (-) (Some 2) (Some 1)




