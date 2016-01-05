let take cnt arr = Seq.toArray (Seq.take cnt arr)

let skipTake cnt arr = 
    let takeAmount = (Array.length arr) - cnt
    let skippedSeq = Seq.skip cnt arr
    Seq.toArray (Seq.take takeAmount skippedSeq)

let binSearch target arr =
    let rec binSearch' target arr count =  
        let middle = (Array.length arr) / 2
        if middle = 0 then
            None
        else
            if target < arr.[middle] then
                binSearch' target (take middle arr) (count + 1)
            else if target > arr.[middle] then
                binSearch' target (skipTake middle arr) (count + 1)
            else 
                Some((target, count))

    binSearch' target arr 0

let search = [|1..10000000|]

// unsafe code, don't use Option.get! could be None...

let (target, iterationsRequired) = Option.get (binSearch 8 search)

