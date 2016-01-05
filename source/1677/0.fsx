module ImmutableDict =
   open System.Collections.Generic
   open System.Collections.Immutable

   let empty<'key,'T> = 
      ImmutableDictionary<'key,'T>.Empty :> IImmutableDictionary<'key,'T>

   let isEmpty (table:IImmutableDictionary<'key,'T>) =
      table.Count = 0

   let add key value (table:IImmutableDictionary<'key,'T>) = 
      table.Add(key,value)
   
   let remove key (table:IImmutableDictionary<'key,'T>) = 
      table.Remove(key)
      
   let containsKey key (table:IImmutableDictionary<'key,'T>) = 
      table.ContainsKey(key)
   
   let find key (table:IImmutableDictionary<'key,'T>) = 
      match table.TryGetKey(key) with
      | true, value -> value
      | false, _ -> raise (KeyNotFoundException())
   
   let tryFind key (table:IImmutableDictionary<'key,'T>) =
      match table.TryGetKey(key) with
      | true, value -> Some value
      | false, _ -> None

   let toArray (table:IImmutableDictionary<'key,'T>) =
      [|for pair in table -> pair.Key, pair.Value|]

   let ofSeq (elements:('key * 'T) seq) =
      seq { for (k,v) in elements -> KeyValuePair(k,v) }
      |> empty.AddRange
     
   let map (mapping:'key -> 'T -> 'U) (table:IImmutableDictionary<'key,'T>) =
      seq { for pair in table -> KeyValuePair<'key,'U>(pair.Key, mapping pair.Key pair.Value) }
      |> empty.AddRange
      
   let filter (predicate:'key -> 'T -> bool) (table:IImmutableDictionary<'key,'T>) =
      seq { for pair in table do if predicate pair.Key pair.Value then yield pair }
      |> empty.AddRange

   let exists (predicate:'key -> 'T -> bool) (table:IImmutableDictionary<'key,'T>) =
      table |> Seq.exists (fun pair -> predicate pair.Key pair.Value)

   let iter (action:'key -> 'T -> unit) (table:IImmutableDictionary<'key,'T>) =
      table |> Seq.iter (fun pair -> action pair.Key pair.Value)