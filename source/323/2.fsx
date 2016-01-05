// Functional quick sort for fun - lazy and incremental.
//
// Even at 5M records, it's faster to use Seq.sort, even if 
// you're going to pick off just 1 element.
let rec qs (pxs:seq<_>) = 
    seq {
        match Seq.toList pxs with
        | p::xs -> let lesser, greater = List.partition ((>=) p) xs
                   yield! qs lesser; yield p; yield! qs greater
        | _ -> ()
    }

let rand=new System.Random()
let data = Array.init 500000 (fun _ -> rand.Next())
#time
data |> qs |> Seq.take 1    // sub second
#time
#time
data |> qs |> Seq.length    // multiple seconds
#time
