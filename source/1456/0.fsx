(* Factorial function *)
let rec (!!) x =
    if x = 1 || x = 0 then 1
    else x * !! (x-1)

(* Choose function *)
let (+?) (n:int) (x:int) =
    !!n / (!!x * !!(n - x))

(*
Binomal probabilities formula 
n is the fixed number of trials.
x is the specified number of successes
n - x is the number of failures
p is probability of sucess on any given trial
*)
let binomal (n:int) (x:int) (p:float) =
    (float (n +? x)) * (p ** (float x)) * ((1.0 - p) ** (float (n - x)))

(* Function for binomial probabilities *)
let ``binomal probabilities`` (data : int list) (p:float) : float list =
    (* We are always involve 0 as input random variable *)
    let n = data.Length - 1
    data
    |> List.map(fun x -> binomal n x p)

let fact3 = !!3
let choose = (3 +? 2)
let b = binomal 3 0 0.30
let p_list = ``binomal probabilities`` [0..3] 0.30 
(* The first result says what is probability of no success in my binomial distribution
   the second result says what is probability of one success in my binomial distribution 
   and so on ... *)