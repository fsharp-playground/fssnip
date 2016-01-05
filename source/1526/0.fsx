// This is an alternative version of the type defined in the F# PR:
// https://visualfsharp.codeplex.com/SourceControl/network/forks/andrewjkennedy/fsharpcontrib/contribution/7632

let rec gcd a (b: int) =
  if b = 0 then a else
    gcd b (a % b)

let lcm a b = 
  (a * b) / gcd a b

type Rational = 
  { numerator: int
    denominator: int }
  static member Create(p, q) = 
    let p, q =
      if q = 0 then raise(System.DivideByZeroException())
      let g = gcd q p in
      p/g, q/g
 
    let p, q =
      if q > 0 then p, q else -p, -q
    
    { numerator = p
      denominator = q }

  static member (+) (m, n) =
    Rational.Create(m.numerator*n.denominator + n.numerator*m.denominator, m.denominator*n.denominator)

  static member (~-) m = 
    Rational.Create(-m.numerator, m.denominator)
 
  static member (*) (m, n) =
    Rational.Create(m.numerator*n.numerator, m.denominator*n.denominator)
 
  static member (/) (m, n) =
    Rational.Create(m.numerator*n.denominator, m.denominator*n.numerator)
 
  static member Abs(m) = 
    Rational.Create(abs m.numerator, m.denominator)
 
  override this.ToString() =
    if this.denominator = 1 then this.numerator.ToString() 
    else sprintf "(%A/%A)" this.numerator this.denominator