module Set = 
  /// Returns all subset of a specified set. For example, for input [1;2;3],
  /// the result will be a set containing sets [1;2;3], [1;2], [1;3], [2;3]
  /// [1], [2], [3] and [].
  let rec subsets s = 
    set [ // Add current set to the set of subsets
          yield s
          // Remove each element and generate subset of 
          // that smaller set
          for e in s do
            yield! subsets (Set.remove e s) ]

// Sample usage
Set.subsets (set [1 .. 3])