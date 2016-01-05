// Based on Andrew's Monotone Chain algorithm, as described here:
// http://en.wikibooks.org/wiki/Algorithm_Implementation/Geometry/Convex_hull/Monotone_chain

type Point = { X: float; Y: float }

let clockwise (p1, p2, p3) =
   (p2.X - p1.X) * (p3.Y - p1.Y)
   - (p2.Y - p1.Y) * (p3.X - p1.X)
   <= 0.0

let rec chain (hull: Point list) (candidates: Point list) =
   match candidates with
   | [ ] -> hull
   | c :: rest ->
      match hull with
      | [ ] -> chain [ c ] rest
      | [ start ] -> chain [c ; start] rest
      | b :: a :: tail -> 
         if clockwise (a, b, c) then chain (c :: hull) rest else
         chain (a :: tail) rest

let hull (points: Point list) =
   match points with
   | [ ] -> points
   | [ _ ] -> points
   | _ ->
       let sorted = List.sort points
       let upper = chain [ ] sorted
       let lower = chain [ ] (List.rev sorted)
       List.append (List.tail upper) (List.tail lower)

// illustration

let a = { X = 0.0; Y = 0.0 }
let b = { X = 2.0; Y = 0.0 }
let c = { X = 1.0; Y = 2.0 }
let d = { X = 1.0; Y = 1.0 }
let e = { X = 1.0; Y = 0.0 }
let test = [a;b;c;d;e]

let h = hull test