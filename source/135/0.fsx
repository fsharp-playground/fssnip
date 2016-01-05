let pascal n = 
  let rec nextrow row y ys =
    match row with
    | x::xs -> nextrow xs x ((x + y)::ys)
    | [] -> 1::ys
  let rec addrow i z zs = 
    if i <= 1 then z::zs
    else addrow (i-1) (nextrow z 0 []) (z::zs)
  addrow n [1] [] 
