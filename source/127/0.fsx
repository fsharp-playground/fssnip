// Generate n rows of Pascal's Triangle
let pascal n = 
  let rec pas L n =
    if n = 0 then L
    else
      let A::t = L in
      pas (((1::[for i in 1..(List.length A-1) -> A.[i-1]+A.[i]])@[1])::L) (n-1)
  pas [[1;1]] n

// Create Pascal's Triangle with 10 rows
pascal 10;;
