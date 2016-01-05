type Nat =
  | Zero
  | Succ of Nat

let rec plus = function
  | (m,Zero) -> m
  | (m, Succ n) -> Succ (plus (m,n));;

//val plus : Nat * Nat -> Nat

//plus (Succ Zero, Succ (Succ Zero));;
//val it : Nat = Succ (Succ (Succ Zero))
let rec mult = function
  | (m,Zero) -> Zero
  | (m,Succ n) -> plus (m, mult(m,n));;

//val mult : Nat * Nat -> Nat

//mult (Succ(Succ Zero), Succ (Succ Zero));;
//val it : Nat = Succ (Succ (Succ (Succ Zero)))

let rec fact = function
  | Zero -> Succ Zero
  | Succ m -> mult (Succ m, fact m);;
//val fact : Nat -> Nat

//fact (Succ (Succ (Succ (Succ (Succ Zero)))));;
//Lots of Succs

let rec ofNat = function
  | Zero -> 0
  | Succ m -> 1 + ofNat m

//val ofNat : Nat -> int
//fact (Succ (Succ (Succ (Succ (Succ Zero))))) |> ofNat;;
//val it : int = 120
