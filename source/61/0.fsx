/// Fibonacci computation using continuations.
/// Something of a continuation classic.
let fib n =
  let rec f n cont =
    match n with
    | 0 -> cont 0
    | 1 -> cont 1
    | n -> 
      f (n-2) (fun n2-> 
        f (n-1) (fun n1->
          cont(n1+n2)))
  f n (fun x->x)


/// Sum of 1-n using continuations.
/// Somewhat simpler to remember.
/// Note: this is only an example,
/// in reality, tail recursion would be 
/// a more appropriate choice here.
let sum1 n =
  let rec f n cont =
    match n with
    | 1 -> cont 1
    | n -> f (n-1) (fun n1->cont(n+n1))
  f n (fun x->x)