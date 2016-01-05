open System
open System.Linq

module Seq =

    /// Partition into groups linearly by predicate (drops the partition element)
    /// ex. partitionLinear (fun x -> x = 1) [2; 3; 4; 5; 1; 2; 3; 4; 5]
    /// val it : seq<int list> = seq [[2; 3; 4; 5]; [2; 3; 4; 5]]
    let partitionLinear (func : _ -> bool) (sequence : _ seq) : _ list seq =
        seq {
            use en = sequence.GetEnumerator ()
            let more = ref true
            while !more do
                let wasGood = ref true
                let sublist = 
                    [
                        while !wasGood && en.MoveNext() do
                            if not (func en.Current) then yield en.Current
                            else wasGood := false
                    ]
                if List.isEmpty sublist then more := false
                else yield sublist
        }

    /// Partition into groups linearly by predicate (partition element inclusive)
    /// ex. partitionLinearInclusive (fun x -> x = 1) [2; 3; 4; 5; 1; 2; 3; 4; 5]
    /// val it : seq<int list> = seq [[2; 3; 4; 5; 1]; [2; 3; 4; 5]]
    let partitionLinearInclusive (func : _ -> bool) (sequence : _ seq) : _ list seq =
        seq {
            use en = sequence.GetEnumerator ()
            let more = ref true
            while !more do
                let wasGood = ref true
                let sublist = 
                    [
                        while !wasGood && en.MoveNext() do
                            if func en.Current then wasGood := false
                            yield en.Current
                    ]
                if List.isEmpty sublist then more := false
                else yield sublist
        }