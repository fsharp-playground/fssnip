open System

/// Random number generator
let rnd = new Random()

/// Generates random walk starting from value 'v'
let rec randomWalk v = 
  seq { // Emit the first value of the walk
        yield v
        // Emit all values starting from new location
        yield! randomWalk (v + rnd.NextDouble()) }

// Initial call to generate random walk from value 0
randomWalk(0.0)
