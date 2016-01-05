open System.Collections.Concurrent
open System.Collections.Generic

module private Memo =
    let memo (cache : unit -> ('a -> 'b) -> ('a -> 'b)) f =
        let cache = cache()
        cache f

    let memoFix cache f =
        let cache = cache()
        let rec fn x = cache (f fn) x
        fn

module DictionaryMemo =    
    let createCache () =
        let dict = Dictionary()
        fun f x -> match dict.TryGetValue (Some x) with
                   | true, value -> value
                   | _ -> let value = f x
                          dict.[Some x] <- value
                          value

    let memo f = Memo.memo createCache f
    let memoFix f = Memo.memoFix createCache f
    let memo2 f = memo (memo << f)
    let memo3 f = memo (memo2 << f)
    let memo4 f = memo (memo3 << f)
    let memo5 f = memo (memo4 << f)
    let memo6 f = memo (memo5 << f)
    let memo7 f = memo (memo6 << f)
    let memo8 f = memo (memo7 << f)

module ConcurrentMemo =
    let createCache () = 
        let dict = ConcurrentDictionary()
        fun f x -> dict.GetOrAdd(Some x, lazy (f x)).Value

    let memo f = Memo.memo createCache f
    let memoFix f = Memo.memoFix createCache f
    let memo2 f = memo (memo << f)
    let memo3 f = memo (memo2 << f)
    let memo4 f = memo (memo3 << f)
    let memo5 f = memo (memo4 << f)
    let memo6 f = memo (memo5 << f)
    let memo7 f = memo (memo6 << f)
    let memo8 f = memo (memo7 << f)