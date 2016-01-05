open System
open System.Linq

module Seq =
    let catchExceptions handler (sequence: _ seq) =
        let e = sequence.GetEnumerator()
        let evaluateNext () = 
            try Some (e.Current)
            with ex -> handler ex 
        seq { while e.MoveNext() do
                  match evaluateNext() with
                  | Some (item) -> yield item 
                  | None -> () }