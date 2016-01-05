// First, let's define some combinators...
let s f g x = f x (g x)
let k x y   = x
let b f g x = f (g x)
let c f g x = f x g
let rec y f x = f (y f) x
let cond p f g x = if p x then f x else g x

// The following functions show the steps to transform 
// original definition of factorial into combinatorial form
let rec fact0 n = if n=0 then 1 else n*fact0(n-1)
let fact1 = y (fun f n -> if n=0 then 1 else n*f(n-1))
let fact2 = y (fun f -> cond ((=)0) (k 1) (s (*) (b f (c(-)1))))
let fact3 = y (b (cond ((=)0) (k 1)) (fun f->(s (*) (b f (c(-)1)))))

// Final version
let fact = y (b (cond ((=) 0) (k 1)) (b (s (*)) (c b (c(-)1))))

fact 5