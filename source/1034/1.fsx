// generic 2-arg currying function
let curry f x y = f(x,y)

// insert given element x into the list at all possible positions
// returns list of lists
let rec ins x = function
   | [] -> [[x]]
   | h::t -> (x::h::t)::(List.map (curry List.Cons h) (ins x t))

// computes all permutations of a given list of elements
// return list of lists
let rec perm = function
  | [] -> [[]]
  | h::t -> List.collect (ins h) (perm t)

// compute the factorian of a given number
// by building the list of permutations of the list 
// of first n numbers [1..n] and taking its length
let fact n = List.length (perm [1..n])

// even better way to write the same thing
let fact' = (..) 1 >> List.ofSeq >> perm >> List.length
