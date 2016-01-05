module Dict = 
    open System.Collections.Generic
    let tryGetDefault_v1 (d:IDictionary<'k,'v>) (key:'k) (defaultValue:'v) =
        if d.ContainsKey(key) then
            d.[key]
        else
            defaultValue

    let tryGetDefault_v2 (d:IDictionary<'k,'v>) (key:'k) (defaultValue:'v) =
        let mutable value = defaultValue
        if d.TryGetValue(key, &value) then
            value
        else
            defaultValue