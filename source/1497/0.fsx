type Change<'a> = 
    | Added of 'a
    | Removed of 'a
    | Modified of current : 'a * previous : 'a

let compare (getKey : 'a -> 'b) (hasChanged : 'a -> 'a -> bool) (current : 'a list) (previous : 'a List) : seq<Change<'a>> = 
    let currentKeys = 
        current
        |> List.map getKey
        |> Set.ofList
    
    let previousKeys = 
        previous
        |> List.map getKey
        |> Set.ofList
    
    let added = 
        current |> Seq.filter (fun item -> 
                       let key = item |> getKey
                       not (previousKeys |> Set.contains key))
    
    let removed = 
        current |> Seq.filter (fun item -> 
                       let key = item |> getKey
                       not (currentKeys |> Set.contains key))
    
    let remainingKeys = Set.intersect currentKeys previousKeys
    let currentRemaining = current |> List.filter (fun x -> remainingKeys |> Set.contains (x |> getKey))
    
    let previousRemainingLookup = 
        previous
        |> Seq.filter (fun x -> remainingKeys |> Set.contains (x |> getKey))
        |> Seq.map (fun x -> getKey x, x)
        |> Map.ofSeq
    seq { 
        for key in added do
            yield Added key
        for key in removed do
            yield Removed key
        for current in currentRemaining do
            let key = getKey current
            let previous = previousRemainingLookup.[key]
            if hasChanged current previous then yield Modified(current, previous)
    }
