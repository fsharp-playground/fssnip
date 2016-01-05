// A faster version of 'http://fssnip.net/ph'. The original function is:
let rec selections1 l = 
  match l with 
  | [] -> []
  | (x::xs) -> (x,xs) :: [for (y,ys) in (selections1 xs) -> (y, x::ys)]

// Rather than iterating over all the remaining elements in the 'x::xs'
// case and appending the 'x' element to all of the recursively produced
// results, we pass around a function 'prefix' that prefixes the list with
// all elements that we stepped over before - this way, we can build the
// final list immediately, return it and be done with it.
let rec selections2 prefix l = 
  match l with 
  | [] -> []
  | (x::xs) -> (x,prefix xs) :: selections2 (fun rest -> prefix (x::rest)) xs

// Stats on my machine:
#time
let input = [1..2000]
// ~770ms
selections1 input |> List.length
// ~270ms
selections2 id input |> List.length