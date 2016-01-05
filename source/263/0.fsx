module Seq =
    let inline toTyped (coll:#System.Collections.IEnumerable) =
        seq {
            for i in 0 .. (^t : (member Count : int) coll) - 1 ->
                (^t : (member get_Item : int -> _) (coll,i))
        }

// Example usage:
System.Text.RegularExpressions.Regex.Matches("abcde","a|c")
|> Seq.toTyped
|> Seq.map (fun m -> m.Value)