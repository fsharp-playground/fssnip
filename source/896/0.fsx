module Seq =

    /// Returns a sequence that yields chunks of length n.
    /// Each chunk is returned as a list.
    let toChunks length (source: seq<'T>) =
        use ie = source.GetEnumerator()
        let sourceIsEmpty = ref false
        let rec loop () =
            seq {
                if ie.MoveNext () then
                    yield [
                            yield ie.Current
                            for x in 2 .. length do
                                if ie.MoveNext() then
                                    yield ie.Current
                                else
                                    sourceIsEmpty := true
                    ]
                    match !sourceIsEmpty with
                    | false -> yield! loop ()
                    | true  -> ()
            }
        loop ()

// Demo
[1 .. 20]
|> Seq.toChunks 3
|> Seq.toArray

// Output
// [|[1; 2; 3]; [4; 5; 6]; [7; 8; 9]; [10; 11; 12]; [13; 14; 15]; [16; 17; 18];
// [19; 20]|]