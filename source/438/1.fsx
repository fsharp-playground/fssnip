module PairingHeap = 
    type Heap<'T> = 
        | Empty
        | Heap of 'T * List<Heap<'T>>
    let findMin heap =
        match heap with
        | Empty -> failwith "findMin called on empty heap"
        | Heap (element, subheaps) -> element
    let merge heap1 heap2 =
        match heap1, heap2 with
        | Empty, Empty -> Empty
        | _, Empty -> heap1
        | Empty, _ -> heap2
        | Heap(elem1, sub1), Heap(elem2, sub2) ->
            match elem1 < elem2 with
            | true -> Heap(elem1, heap2::sub1)
            | false -> Heap(elem2, heap1::sub2)
    let insert element heap =
        merge (Heap(element, [])) heap
    let rec mergePairs heap =
        match heap with
        | [] -> Empty
        | [h] -> h
        | h1::h2::hs -> merge (merge h1 h2) (mergePairs hs)
    let deleteMin (heap : Heap<'T>) =
        match heap with
        | Empty -> failwith "deleteMin called on Heap.Empty"
        | Heap (element, []) -> Empty
        | Heap (element, [subHeap]) -> subHeap
        | Heap (element, subHeaps) -> mergePairs subHeaps