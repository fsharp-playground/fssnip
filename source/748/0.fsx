let memoizeConcurrent (f : 'a -> 'b) =
    let dict =
        new System.Collections.Concurrent.ConcurrentDictionary<'a, Lazy<'b>>()
    fun x -> dict.GetOrAdd(x, lazy (f x)).Force()