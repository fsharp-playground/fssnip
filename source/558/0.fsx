// [snippet:Implementation]
module Seq =
/// Iterates over the elements of the input sequence and groups adjacent
/// elements. A new group is started after the specified predicate holds 
/// about the element of the sequence (and at the beginning of the iteration).
    let groupAfter f (input:seq<_>) = 
        use en = input.GetEnumerator()
        let rec group() =
            [   yield en.Current
                if not(f en.Current) && en.MoveNext() then
                    yield! group() ]
        seq{
            while en.MoveNext() do
                yield group() |> Seq.ofList }
// [/snippet]
// [snippet:Example]
[3;3;2;4;1;2] |> Seq.groupAfter (fun n -> n%2 = 1);;
/// val it : seq<seq<int>> = seq [[3]; [3]; [2; 4; 1]; [2]]
// [/snippet]