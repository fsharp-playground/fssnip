namespace Foo

  module HashSet =
    
    let add item (set:System.Collections.Generic.HashSet<_>) =
      set.Add(item) |> ignore
      set