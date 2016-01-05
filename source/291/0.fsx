[<Measure>] type cm

type Num<[<Measure>] 'M, [<Measure>] 'N> = 
  | O_ of int * float<'N>
  | S_ of int * Num<'M, 'N / 'M>
  
let O : Num<'M, 'M> = O_ (1, 0.0<_>)
let S n = match n with O_(i, _) | S_(i, _) -> S_(i + 1, n)

let pow (x:float<'M>) ((O_(i, _) | S_(i, _)):Num<'M, 'M 'N>) : float<'M 'N> =
  unbox ((float x) ** float i)

let res = pow 2.0<cm> (S (S O))