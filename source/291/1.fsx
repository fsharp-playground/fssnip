[<Measure>] type cm

// Represents a number with units of measure powered to the
// number's value (e.g "(S (S O))" has type Num<cm, cm^3>)
type Num<[<Measure>] 'M, [<Measure>] 'N> = 
  | O_ of int * float<'N>
  | S_ of int * Num<'M, 'N / 'M>

// Constructors that hide that simplify the creation  
let O : Num<'M, 'M> = O_ (1, 0.0<_>)
let S n = match n with O_(i, _) | S_(i, _) -> S_(i + 1, n)

// Type-safe power function with units of measure
let pow (x:float<'M>) ((O_(i, _) | S_(i, _)):Num<'M, 'M 'N>) : float<'M 'N> =
  // Unsafe hacky implementation, which is hidden
  // from the user (for simplicity)
  unbox ((float x) ** float i)

let res = pow 2.0<cm> (S (S O))