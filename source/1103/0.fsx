let rec MergeSort (input: int list) = 
    
    let Merge (left: int list, right: int list) =
        let rec mergeLists (left: int list, right: int list, output: int list) =
            if left = [] then output@right
            else if right = [] then output@left
            else if left.Head < right.Head then mergeLists (left.Tail, right, output@[left.Head])
            else mergeLists (left, right.Tail, output@[right.Head])
        mergeLists (left, right, [])

    if input.Length = 0 then []
    else if input.Length = 1 then input
    else if input.Length = 2 then
        if input.[0] > input.[1] then [input.[1]; input.[0]]
        else input
    else
        let left = MergeSort (input |> Seq.take (input.Length / 2) |> Seq.toList)
        let right = MergeSort (input |> Seq.skip (input.Length / 2) |> Seq.toList)
        Merge (left, right)