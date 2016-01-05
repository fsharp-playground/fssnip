// Note that Dictionary lookups are to be much faster than F#'s map and so should
// be used when writing library code.

let memoizeBy inputToKey f =
    let cache = Dictionary<_, _>()
    fun x ->
        let k = inputToKey x
        if cache.ContainsKey(k) then cache.[k]
        else let res = f x
             cache.[k] <- res
             res

// Example: 

open System

/// Caches calls to Type.GetGenericArguments
let cachedGetGenericArguments : Type -> Type[] =
    memoizeBy (fun t -> t.FullName) (fun t -> t.GetGenericArguments())
