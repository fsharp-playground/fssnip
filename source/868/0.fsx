type Hashtable<'TKey,'TValue> when 'TKey : equality 
        (capacity:int, items:('TKey * 'TValue) seq)  =
    let table = Array.create capacity (ResizeArray<_>())
    let indexOf key = (hash key)%capacity
    do  for key, value in items do table.[indexOf key].Add(key,value)
    new (items) = Hashtable(Seq.length items, items)
    member this.TryGetValue(key:'TKey) =       
        table.[indexOf key] 
        |> Seq.tryFind (fst >> (=) key) 
        |> Option.map snd