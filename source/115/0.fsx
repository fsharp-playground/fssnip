module Simple = 
  // [snippet:Composing existing functions]
  let everyNth n seq = 
    seq |> Seq.mapi (fun i el -> el, i)              // Add index to element
        |> Seq.filter (fun (el, i) -> i % n = n - 1) // Take every nth element
        |> Seq.map fst                               // Drop index from the result
  // [/snippet]

module Efficient = 
  // [snippet:Efficient version using enumerator]
  let everyNth n (input:seq<_>) = 
    seq { use en = input.GetEnumerator()
          // Call MoveNext at most 'n' times (or return false earlier)
          let rec nextN n = 
            if n = 0 then true
            else en.MoveNext() && (nextN (n - 1)) 
          // While we can move n elements forward...
          while nextN n do
            // Retrun each nth element
            yield en.Current }
  // [/snippet]