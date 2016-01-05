module Seq =
    let paritionWhile (pred : _ -> bool) (sequence : _ seq) : _ list * _ seq = 
        let en = sequence.GetEnumerator ()
        let wasGood = ref true
        let sublist = 
            [
                while !wasGood && en.MoveNext() do
                    if pred en.Current then yield en.Current
                    else wasGood := false
            ]
        let remainder = 
            seq { 
                    if not !wasGood then yield en.Current
                    while en.MoveNext() do yield en.Current 
                }
        sublist, remainder

[<Fact>]
let subsetUntil_ShouldReturnProperSubset () =
    let testSeq = seq { for i in 1 .. 6 do yield i }
    let sub, remainder = testSeq |> Seq.partitionWhile (fun x -> x <= 3)
    Assert.Equal( [1; 2; 3], sub )
    Assert.Equal( [4; 5; 6], remainder |> Seq.toList )

[<Fact>] 
let subsetUntil_shouldReturnEmptyListandSeqWhenEmptyGiven () =
    let testSeq = Seq.empty
    let sub, remainder = testSeq |> Seq.partitionWhile (fun x -> x <= 3)
    Assert.Empty sub
    Assert.Empty remainder