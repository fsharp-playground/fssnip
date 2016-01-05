[<AutoOpen>]
module EnToSeq =
    type Linq.QueryBuilder with
        member inline qb.Source(s) = 
            let e = (^t : (member GetEnumerator : unit -> ^e) s)
            Linq.QuerySource<_,System.Collections.IEnumerable>(
                seq { while (^e : (member MoveNext : unit -> bool) e) do
                        yield (^e : (member Current : ^v) e) })

[<AutoOpen>]
module EnToSeqWithItem =
    type Linq.QueryBuilder with
        member inline qb.Source(s) = 
            // constrain ^t to have an Item indexed property (which we don't actually invoke)
            let _ = fun x -> (^t : (member Item : int -> ^v with get) (x, 0))
            let e = (^t : (member GetEnumerator : unit -> ^e) s)
            Linq.QuerySource<_,System.Collections.IEnumerable>(
                seq { while (^e : (member MoveNext : unit -> bool) e) do
                        yield (^e : (member Current : obj) e) :?> ^v })

// demonstration usage
query { for m in System.Text.RegularExpressions.Regex.Matches("test", "t|e") do
        where m.Success
        select m.Value
        distinct }