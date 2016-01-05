let partition inclusive f (xs : #seq<_>) =
      let iter = xs.GetEnumerator()
      let rec loop () =
         seq {
            let rec innerLoop more =
               [
                  if more then
                     if not <| f iter.Current then
                        yield iter.Current
                        yield! innerLoop (iter.MoveNext())
                     elif inclusive then
                        yield iter.Current
               ]
            while iter.MoveNext() do
               yield innerLoop true
         }
      loop ()

module Seq =
    /// Partition into groups linearly by predicate (drops the partition element)
    /// ex. partitionLinear (fun x -> x = 1) [2; 3; 4; 5; 1; 2; 3; 4; 5]
    /// val it : seq<int list> = seq [[2; 3; 4; 5]; [2; 3; 4; 5]]
    let partitionLinear f xs = partition false f xs
    /// Partition into groups linearly by predicate (partition element inclusive)
    /// ex. partitionLinearInclusive (fun x -> x = 1) [2; 3; 4; 5; 1; 2; 3; 4; 5]
    /// val it : seq<int list> = seq [[2; 3; 4; 5; 1]; [2; 3; 4; 5]]
    let partitionLinearInclusive f xs = partition true f xs