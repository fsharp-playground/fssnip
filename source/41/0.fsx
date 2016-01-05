type Indexable() =  
  /// Returns a 2D slice. Arguments are optional and will be None
  /// when the user writes for example x.[1 .. , 1 .. ]  
  member x.GetSlice(start1, finish1, start2, finish2) = 
    let s1, f1 = defaultArg start1 0, defaultArg finish1 100
    let s2, f2 = defaultArg start2 0, defaultArg finish2 100
    // Return a string with the range of the slice
    sprintf "[%A, %A] -> [%A, %A]" s1 s2 f1 f2 
 
// Returns [1, 0] -> [100, 10]
let r = new Indexable()     
r.[1.., ..10]

