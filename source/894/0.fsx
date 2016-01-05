/// Define SumOfSquares computation builder
type SumOfSquaresMonoid() =
  /// Combine two values
  /// sm.Combine («cexpr1», b.Delay(fun () -> «cexpr2»))
  member sm.Combine(a,b) = a + b
  /// Zero value
  /// sm.Zero()
  member sm.Zero() = 0.0
  /// Return a value 
  /// sm.Yield expr
  member sm.Yield(a) = a
  /// Delay a computation
  /// sm.Delay (fun () -> «cexpr»))
  member sm.Delay f = f()
  /// For loop
  /// sm.For (expr, (fun pat -> «cexpr»))
  member sm.For(e, f) =
    Seq.fold(fun s x -> sm.Combine(s, f x)) (sm.Zero()) e

// Create an instance of each such monoid object
let sosm = new SumOfSquaresMonoid()

// Build a SumOfSquaresMonoid value(function)
let sumOfSquares x = sosm {for x in [1.0 .. 0.2 .. x] do yield x * x}

// Evaluation
sumOfSquares 10.2

// Result
// val it : float = 1819.84