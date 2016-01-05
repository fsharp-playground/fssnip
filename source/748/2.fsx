open System.Collections.Concurrent
let memoizeConcurrent f =
    let dict = ConcurrentDictionary()
    fun x -> dict.GetOrAdd(Some x, lazy (f x)).Force()