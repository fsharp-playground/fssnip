// Partitions a list into a map of key->List<T> based on the 
// key returned by the call to discriminatorFunc
// Note that lists are in reversed order to the input...
let ListPartition (discriminatorFunc : 'V -> 'K) list = 
    let rec part (map:System.Collections.Generic.Dictionary<'K,'V list>) listTail =
        match listTail with
        | [] -> map
        | x :: tail -> let key = discriminatorFunc x
                       let res,list =  map.TryGetValue(key)
                       match res with
                       | false -> map.Add(key,[x])
                       | true -> map.[key] <- x :: list
                       part map tail

    part (new System.Collections.Generic.Dictionary<'K,'V list>()) list