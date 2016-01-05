open System.Collections.Generic

let (|Dict|) (d : Dictionary<_,_>) = d

let tryFind (Dict d) k = if d.ContainsKey k then Some <| d.[k] else None