module PairingHeap = 
    type Heap<'T> = Heap of List<Heap<'T>>
    let findMin heap =
        match heap with
        | Heap([]) -> Heap([])
        | Heap(element::_) -> Heap([element])
    let merge heap1 heap2 =
        match heap1, heap2 with
        | Heap([]), Heap([]) -> Heap([])
        | Heap([]), _ -> heap2
        | _, Heap([]) -> heap1
        | Heap(elem1::sub1), Heap(elem2::sub2) ->
            match elem1 < elem2 with
            | true -> Heap(elem1::elem2::sub2@sub1)
            | false -> Heap(elem2::elem1::sub1@sub2)
    let insert element heap =
        merge (Heap([element])) heap
    let rec mergePairs heap =
        match heap with
        | Heap ([]) -> Heap([])
        | Heap ([h]) -> Heap([h])
        | Heap (h1::h2::hs) -> merge (merge h1 h2) (mergePairs heap)
    let deleteMin (heap : Heap<'T>) =
        let unfurl (Heap(heapList)) = heapList
        match heap with
        | Heap ([]) -> Heap([])
        | Heap (elem::[]) -> Heap([])
        | Heap (elem::newElement::[]) -> newElement
        | Heap (element::newElement::subheaps) -> 
            Heap(newElement::(unfurl (mergePairs (Heap(subheaps)))))