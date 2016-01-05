open System
open System.Threading

//quick sort
let quickSort (a : 'a[]) =
    let rand = new Random() //for random pivot choice
    //swaps elements of array a with indices i and j
    let swap (a : 'a[]) i j =
        let temp = a.[i] 
        a.[i] <- a.[j]
        a.[j] <- temp
    
    //sorts subarray [l; r) of array a in-place
    let rec quickSortRange (a : 'a[]) l r =
        match r - l with
        | 0 | 1 -> ()
        | n ->        
            //preprocess: swap pivot to 1st position
            swap a l <| rand.Next(l, r)
            let p = a.[l]
            //scan and partitioning
            let mutable i = l + 1 //left from i <=> less than pivot part 
            for j in (l+1)..(r-1) do
                //preserve invariant: [p|  <p |i >p  |j  unpartitioned  ]
                if a.[j] < p then
                    swap a j i
                    i <- i + 1
            swap a (i-1) l //place pivot in appropriate pos.
            let iImmutable = i //instead of using ref cells
            ThreadPool.QueueUserWorkItem(fun _ -> quickSortRange a l (iImmutable-1)) |> ignore
            ThreadPool.QueueUserWorkItem(fun _ -> quickSortRange a iImmutable r) |> ignore

    quickSortRange a 0 a.Length