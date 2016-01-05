namespace Foo

  module Collection =
    
    let add (set:System.Collections.Generic.ICollection<_>) item =
      set.Add(item) |> ignore

    // EXAMPLE
    let testSet = new System.Collections.Generic.HashSet<_>()
    let test = [1..10] |> List.iter (add testSet)
