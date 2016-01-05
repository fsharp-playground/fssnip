open System

/// Calculates whether a point is within the unit circle
let insideCircle (x:float, y:float) = 
  // TASK #1: Implement the test! 
  false

/// Generates random X, Y values between 0 and 1
let randomPoints max = seq {
  let rnd = new Random()
  for i in 0 .. max do
    yield rnd.NextDouble(), rnd.NextDouble() }

/// Generate specified number of random points and
/// calculate PI using Monte Carlo simulation
let monteCarloPI size = 
  // TASK #2: Generate specified number of random
  // points and test how many are inside circle
  // (...)
  let inside = 0 
  // Estimate the value of PI
  float inside / float size * 4.0

// Test the Monte Carlo PI calculation
#time
monteCarloPI 1000000

// Run the calculation 10 times and calculate average
// Change to 'Array.Parallel.map' to parallelize!
[| for i in 0 .. 10 -> 1000000 |]
|> Array.map monteCarloPI
|> Array.average