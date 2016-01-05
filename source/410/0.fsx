open System

// [snippet:Permutation and Combination using ListBuilder]
type ListBuilder() =
  let concatMap f m = List.concat( List.map (fun x -> f x) m )
  member this.Bind (m, f) = concatMap (fun x -> f x) m 
  member this.Return (x) = [x]
  member this.ReturnFrom (x) = x
  member this.Zero () = []
  member this.Combine (a,b) = a@b
  member this.Delay f = f ()

let list = ListBuilder()

let rec permutations n lst = 
  let rec selections = function
      | []      -> []
      | x::xs -> (x,xs) :: list { let! y,ys = selections xs 
                                  return y,x::ys }
  (n, lst) |> function
  | 0, _ -> [[]]
  | _, [] -> []
  | _, x::[] -> [[x]]
  | n, xs -> list { let! y,ys = selections xs
                    let! zs = permutations (n-1) ys 
                    return y::zs }

let rec combinations n lst = 
  let rec findChoices = function 
    | []    -> [] 
    | x::xs -> (x,xs) :: list { let! y,ys = findChoices xs 
                                return y,ys } 
  list { if n = 0 then return! [[]]
         else
           let! z,r = findChoices lst
           let! zs = combinations (n-1) r 
           return z::zs }
// [/snippet]

// [snippet:Example]
let x4P0 = permutations 0 [1;2;3;4]
printfn "4P0 = %d" x4P0.Length
x4P0 |> Seq.iter (fun x -> printfn "%A" x)
Console.WriteLine ("-----") |> ignore

let x4P2 = permutations 2 [1;2;3;4]
printfn "4P2 = %d" x4P2.Length
x4P2 |> Seq.iter (fun x -> printfn "%A" x)
Console.WriteLine ("-----") |> ignore

let x4C0 = combinations 0 [1;2;3;4]
printfn "4C0 = %d" x4C0.Length
x4C0 |> Seq.iter (fun x -> printfn "%A" x)
Console.WriteLine ("-----") |> ignore

let x4C2 = combinations 2 [1;2;3;4]
printfn "4C2 = %d" x4C2.Length
x4C2 |> Seq.iter (fun x -> printfn "%A" x)
Console.ReadLine () |> ignore
// [/snippet]
