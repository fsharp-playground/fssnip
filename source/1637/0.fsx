    let inline memoize f =
        let dict = System.Collections.Concurrent.ConcurrentDictionary()
        fun x -> async {
            match dict.TryGetValue x with
            | true, result -> return result
            | false, _ ->
                let! result = f x
                dict.TryAdd(x, result) |> ignore
                return result
        }