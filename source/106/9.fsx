module Seq =
    /// Takes elements into a sublist until the predicate returns false (exclusive)
    let partitionWhile (func : _ -> bool) (sequence : _ seq) : _ list * _ seq = 
        let en = sequence.GetEnumerator ()
        let wasGood = ref true
        let sublist = 
            [
                while !wasGood && en.MoveNext() do
                    if func en.Current then yield en.Current
                    else wasGood := false
            ]
        let remainder = 
            seq { 
                    if not !wasGood then yield en.Current
                    while en.MoveNext() do yield en.Current 
                }
        sublist, remainder

    ///Takes elements into a sublist until the predicate returns true (exclusive)
    let partitionUntil (func : _ -> bool) (sequence : _ seq) : _ list * _ seq = 
        let en = sequence.GetEnumerator ()
        let satisfied = ref false
        let sublist = 
            [
                while not !satisfied && en.MoveNext() do
                    if not (func en.Current) then yield en.Current
                    else satisfied := true
            ]
        let remainder = 
            seq { 
                    if !satisfied then yield en.Current
                    while en.MoveNext() do yield en.Current 
                }
        sublist, remainder

    ///Takes elements into a sublist until the predicate returns true (inclusive)
    let partitionUntilAfter (func : _ -> bool) (sequence : _ seq) : _ list * _ seq = 
        let en = sequence.GetEnumerator ()
        let satisfied = ref false
        let sublist = 
            [
                while not !satisfied && en.MoveNext() do
                    if func en.Current then satisfied := true
                    yield en.Current
            ]
        let remainder = 
            seq { 
                    while en.MoveNext() do yield en.Current 
                }
        sublist, remainder

[<Fact>]
let ``Seq.partitionWhile should return a proper subset and remainder`` () =
    let testSeq = seq { for i in 1 .. 6 do yield i }
    let sub, remainder = testSeq |> Seq.partitionWhile (fun x -> x <= 3)
    Assert.Equal( [1; 2; 3], sub )
    Assert.Equal( [4; 5; 6], remainder |> Seq.toList )

[<Fact>] 
let ``Seq.partitionWhile should return an empty list and remainder when give an empty sequence`` () =
    let testSeq = Seq.empty
    let sub, remainder = testSeq |> Seq.partitionWhile (fun x -> x <= 3)
    Assert.Empty sub
    Assert.Empty remainder

[<Fact>]
let ``Seq.partitionUntil should return a proper subset and remainder`` =
    let testSeq = seq { for i in 1 .. 6 do yield i }
    let sub, remainder = testSeq |> Seq.partitionUntil (fun x -> x > 3)
    Assert.Equal( [1; 2; 3], sub )
    Assert.Equal( [4; 5; 6], remainder |> Seq.toList )

[<Fact>] 
let ``Seq.partitionUntil should return an empty list and remainder when give an empty sequence`` () =
    let testSeq = Seq.empty
    let sub, remainder = testSeq |> Seq.partitionUntil (fun x -> x > 3)
    Assert.Empty sub
    Assert.Empty remainder

[<Fact>]
let ``Seq.partitionUntilAfter should return a proper subset and remainder`` () =
    let testSeq = seq { for i in 1 .. 6 do yield i }
    let sub, remainder = testSeq |> Seq.partitionUntilAfter (fun x -> x > 2)
    Assert.Equal( [1; 2; 3], sub )
    Assert.Equal( [4; 5; 6], remainder |> Seq.toList )

[<Fact>] 
let ``Seq.partitionUntilAfter should return an empty list and remainder when give an empty sequence`` () =
    let testSeq = Seq.empty
    let sub, remainder = testSeq |> Seq.partitionUntilAfter (fun x -> x > 2)
    Assert.Empty sub
    Assert.Empty remainder