let solve a b c =
  let D = b*b-4.*a*c in
  [(+);(-)] |> List.map (fun f -> (f -b (sqrt D))/2./a)

solve 1.0 2.0 -3.0