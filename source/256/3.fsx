// [snippet:Combinatorial functions]
// Combinatorial functions from the book "Introduction to Functional Programming"
// by Richard Bird and Philip Wadler.

// the function inits returns the list of all inital segments of a list, 
// in order of increasing length.
let rec inits = function
    | []    -> [[]]
    | x::xs -> [ yield [];
                 for ys in inits xs do
                    yield! [x::ys]]
// Example:
// > inits [1..5];;
// val it : int list list =
//  [[]; [1]; [1; 2]; [1; 2; 3]; [1; 2; 3; 4]; [1; 2; 3; 4; 5]]

// the function subs returns a list of all subsequences of a list
// this function is also know as the powerset.
let rec subs = function
    | [] -> [[]]
    | x::xs -> [ for ys in subs xs do
                    yield! [ys;x::ys] ]
// > subs [1..3];;
// val it : int list list =
//   [[]; [1]; [2]; [1; 2]; [3]; [1; 3]; [2; 3]; [1; 2; 3]]

// the term interleave x ys returns a  list of all possible ways of inserting 
// the element x into the list ys.
let rec interleave x = function
    | [] -> [[x]]
    | y::ys -> 
        [ yield x::y::ys
          for zs in interleave x ys do
            yield! [y::zs]]

// Example: 
// > interleave "" ["Count"; "Of"; "Monte"; "Cristo"];;
// val it : string list list =
//   [[""; "Count"; "Of"; "Monte"; "Cristo"];
//    ["Count"; ""; "Of"; "Monte"; "Cristo"];
//    ["Count"; "Of"; ""; "Monte"; "Cristo"];
//    ["Count"; "Of"; "Monte"; ""; "Cristo"];
//    ["Count"; "Of"; "Monte"; "Cristo"; ""]]

// the function perms returns a list of all permutations of a list.
let rec perms = function
    | [] -> [[]]
    | x::xs -> List.concat (List.map (interleave x) (perms xs))

// > perms [1..3];;
// val it : int list list =
//   [[1; 2; 3]; [2; 1; 3]; [2; 3; 1]; [1; 3; 2]; [3; 1; 2]; [3; 2; 1]]

// some helper functions
let curry f a b = f(a,b)
let cons x = curry List.Cons x

// the function parts returns a list of all proper partitions of a list.    
let rec parts = function
    | []    -> [[]]
    | [x]   -> [[[x]]]
    | x::x'::xs -> List.map (glue x) (parts (x'::xs)) 
                    @ List.map (cons [x]) (parts (x'::xs))

and glue x xss = (x :: List.head xss):: List.tail xss

// > parts [1..3];;
// val it : int list list list =
//   [[[1; 2; 3]]; [[1; 2]; [3]]; [[1]; [2; 3]]; [[1]; [2]; [3]]]

// This one comes from the book "Programming in Haskell" by Graham Hutton.
// The function choices returns all the way you can select elements from a list
let choices xs = List.concat (List.map perms (subs xs))

// > choices [1..3];;
// val it : int list list =
//   [[]; [1]; [2]; [1; 2]; [2; 1]; [3]; [1; 3]; [3; 1]; [2; 3]; [3; 2];
//    [1; 2; 3]; [2; 1; 3]; [2; 3; 1]; [1; 3; 2]; [3; 1; 2]; [3; 2; 1]]
// [/snippet]
