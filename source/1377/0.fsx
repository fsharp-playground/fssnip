open System.Collections.Concurrent
let rec memoFix f =
    let dict = ConcurrentDictionary()
    let rec fn x = dict.GetOrAdd(Some x, lazy (f fn x)).Value
    fn