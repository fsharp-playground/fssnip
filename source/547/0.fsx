module Simple = 

  // [snippet:Recursive Fibonacci]
  /// Inefficient recursive implementation of Fibonacci numbers.
  /// 
  /// The problem is that 'fibs 3' will call 'fibs 1' recursively
  /// three times. To solve this, we'd like to keep the result of
  /// previously calculated function calls using dynamic programming.
  let rec fibs n = 
    if n < 1 then 1 else
    (fibs (n - 1)) + (fibs (n - 2))
  // [/snippet]

module Memoized =

    // [snippet:Reusable memoization function]
    open System.Collections.Generic
    
    /// The function creates a function that calls the argument 'f'
    /// only once and stores the result in a mutable dictionary (cache)
    /// Repeated calls to the resulting function return cached values.
    let memoize f =    
      // Create (mutable) cache that is used for storing results of 
      // for function arguments that were already calculated.
      let cache = new Dictionary<_, _>()
      (fun x ->
          // The returned function first performs a cache lookup
          let succ, v = cache.TryGetValue(x)
          if succ then v else 
            // If value was not found, calculate & cache it
            let v = f(x) 
            cache.Add(x, v)
            v)
    // [/snippet]

    // [snippet:Memoized Fibonacci]
    /// Recursive function that implements Fibonacci using memoization.
    /// Recursive calls are made to the memoized function, so previously
    /// calculated values are retrieved from the cache.
    let rec fibs = memoize (fun n ->
      if n < 1 then 1 else
      (fibs (n - 1)) + (fibs (n - 2)))

    // Note - add #nowarn "40" to disable warning complaining about recursive
    // value reference. This is not an issue in this snippet.
    // [/snippet]