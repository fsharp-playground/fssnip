// converts a list of sequences to a sequence of lists
// when one sequence is exhausted any remaining elements in the other sequences are ignored
let zipseq (sequencelist:list<seq<'a>>) = 
    let enumerators = sequencelist |> List.map (fun (s:seq<'a>) -> (s.GetEnumerator()))
    seq {
        let hasNext() = enumerators |> List.exists (fun e -> not (e.MoveNext())) |> not
        while hasNext() do
            yield enumerators |> List.map (fun e -> e.Current)
    }

let sample() = 
    let one23 = zipseq [seq {1 .. 3}; seq {1 .. 4}; seq{1 .. 3}]
    printfn "%A" one23 // prints: seq [[1; 1; 1]; [2; 2; 2]; [3; 3; 3]]