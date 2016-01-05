open System.Collections.Generic

type HashMap<'K, 'V when 'K: comparison>(cmp: IEqualityComparer<'K>) =
  let spineLength = 524287
  let spine = Array.create spineLength Map.empty

  member this.Count =
    Array.sumBy Seq.length spine

  member this.Index key =
    abs(cmp.GetHashCode key % spineLength)

  member this.Add(key, value) =
    let idx = this.Index key
    spine.[idx] <- Map.add key value spine.[idx]

  member this.Remove key =
    let idx = this.Index key
    spine.[idx] <- Map.remove key spine.[idx]

  member this.TryGetValue(key, value: byref<'V>) =
    let bucket = spine.[this.Index key]
    match Map.tryFind key bucket with
    | None -> false
    | Some v ->
        value <- v
        true
