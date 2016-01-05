open System

let dist (x1,y1) (x2,y2) = 
 Math.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2))

let closestBf points =
 let n = Seq.length points
 let list = points |> Seq.toList
 seq { for i in 0..n-2 do
         for j in i+1..n-1 do
           yield list.[i], list.[j] }
 |> Seq.minBy (fun (a, b) -> dist a b)
 

let rec closestInternal points = 
 match points with
 | _ when points |> Seq.length < 4 -> closestBf points
 | _ -> 
 //partition points about a vertical line
 let sorted = points |> Seq.sortBy(fun (x,y) -> x)
 let left = sorted |> Seq.take((points |> Seq.length)/2)
 let right = sorted |> Seq.skip((points |> Seq.length)/2)
 
 //recurse each side of the vertical line
 let lMin = closestInternal left
 let rMin = closestInternal right
 
 //find minimum distance between closest pairs on each side of the line
 let lDist =
  match lMin with
  | (a,b) -> dist a b
 
 let rDist = 
  match rMin with
  | (a,b) -> dist a b
  
 let minDist = Math.Min(lDist,rDist)
 let dividingX = left |> Seq.toList |> List.rev |> List.head |> fst
 
 //find close points on the right to the dividing line
 let closePoints = 
  right 
  |> Seq.takeWhile(fun (x,y) -> x - dividingX < minDist) 
  |> Seq.sortBy(fun (x,y) -> y)
  
 //take the close points and merge them with the close points to the dividing line on the left hand side
 let pairs = 
  left 
  |> Seq.skipWhile(fun (x,y) -> dividingX - x > minDist) 
  |> Seq.collect(fun (x,y) -> 
   closePoints 
   |> Seq.skipWhile(fun (x1,y1) -> y1 < y - minDist) 
   |> Seq.takeWhile(fun (x2,y2) -> y2 < y + minDist) 
   |> Seq.map(fun a -> ((x,y),a))) 
  |> Seq.toList
 
 //return the closest pair of points from the three groups
 pairs |> List.append [lMin;rMin] |> List.sortBy(fun (a,b) -> dist a b) |> List.head