module Seq =
    let paritionUntil pred (sequence : _ seq) : _ list * _ seq = 
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