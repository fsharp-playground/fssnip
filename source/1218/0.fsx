module Seq =
    let mapi64 f s = seq {
        let i64 = ref 0L
        for item in s do
            yield (f i64.Value item)
            i64 := !i64 + 1L
    }