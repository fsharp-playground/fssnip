(* Peano constructs *)

type Num = 
  | Zero           (* zero = {} *)
  | Succ of Num    (* or a successor of n = {0, 1, 2, ..., n-1} *)

let rec plus x y =
    match y with
    | Zero    -> x
    | Succ y' -> Succ (plus x y')

plus Zero (Succ Zero) /// -> Succ Zero
plus (Succ (Succ Zero)) (Succ Zero) /// Succ Succ Succ Zero = 3

let rec mult x y =
    match y with
    | Zero -> Zero
    | Succ y' -> plus x (mult x y')

mult (Succ Zero) (Succ (Succ (Succ Zero)))
mult (Succ (Succ Zero)) (Succ (Succ Zero))