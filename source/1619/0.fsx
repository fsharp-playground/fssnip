let combos (ints : int seq) =
    // unfold takes a generator function instead of being recursive.  
    // It wants you to return either None signalling to stop genning, 
    // or Some (thing to return, leftover state) to signal continuing generation.
    // this all happens on demand
    Seq.unfold (
        fun (itemsleft) -> 
            if Seq.isEmpty itemsleft then None 
            else 
                let head = itemsleft |> Seq.head
                let tail = itemsleft |> Seq.tail
                let newPairs = tail |> Seq.map (fun i -> head,i )
                Some(newPairs, tail)) 
        (ints)
    // so we now have a seq of seq<int*int>.  collect stiches them together
    |> Seq.collect id

let isSumPossible target ints = 
    combos ints
    |> Seq.exists (fun (i,j) -> i + j = target)
    
let testRandom () =
    let rand = new System.Random()
    let amount = rand.Next(2, 1000);
    let sum = rand.Next()
    let ints = Seq.init amount (fun i -> rand.Next())
    ints
    |> isSumPossible sum