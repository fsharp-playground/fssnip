(*************************************************)
(*************************************************)

(** Bad reverse in O(n^2) **)
let rec revb = function
    | [] -> []
    | h::t -> (revb t) @ [h]

revb [1;2;3]


(** Reverse with TAIL RECURSION in O(n) **)
let rev L =
  let rec rev' t = function
    | [] -> t
    | x::z -> rev' (x::t) z
  rev' [] L

rev [1;2;3]

(*************************************************)
(*************************************************)

(****** TREES *******)

(* Type definition *)
type 't tree = Leaf of 't | Node of 't * ('t tree list)

(* Example tree *)
let T = Node (1, [Node(2, [Leaf(3); Leaf(4)]); Leaf(5)])

    
(* Tree traversal *)
let rec iterh f h = function
    | Leaf(x) -> f h x
    | Node(x,L) -> f h x; List.iter (fun x -> iterh f (h+1) x) L

(* Nodes sum *)
let rec sum = function
    | Leaf(x) -> x
    | Node(x, L) -> x + List.fold (fun a node -> a + (sum node)) 0 L

sum T

(********* BINARY TREES *********)
type 't btree = Nil | BNode of 't * 't btree * 't btree

let Leaf x = BNode(x, Nil, Nil) (** Creating a leaf **)

let expr = BNode("+", BNode("*", Leaf("1"), Leaf("2")), Leaf("3")) (** "1*2 + 3" **)

type Traversal = Infix | Prefix | Postfix

(* Traversal infix/prefix/postfix *)
(* BAD *)
let rec iterb tt f = function
    | Nil -> () (* unit *)
    | BNode(x, L, R) ->
        match tt with
        | Prefix -> f x; iterb tt f L; iterb tt f R
        | Infix -> iterb tt f L; f x; iterb tt f R
        | Postfix -> iterb tt f L; iterb tt f R; f x

(* BETTER *)
let Prefix = fun a b c -> a(); b(); c()
let Infix = fun a b c -> b(); a(); c()
let Postfix = fun a b c -> b(); c(); a()

let rec iter tt f = function
    | Nil -> () (* unit *)
    | BNode(x, L, R) ->
        tt (fun ()-> f x) (fun ()-> iter tt f L) (fun ()-> iter tt f R)

let rec fold_infix f a0 = function
    | Nil -> a0
    | BNode (x,L,R) ->
        let a1 = fold_infix f a0 L
        let a2 = f a1 x
        fold_infix f a2 R

fold_infix (fun x y -> y::x) [] expr

(** BINARY SEARCH TREE OPERATIONS **)
let rec insert x = function
    | Nil -> Leaf(x)
    | BNode(z,L,R) ->
        if x<=z then BNode(z, (insert x L),R)
        else BNode(z,L,(insert x R))

let randList = [
                let R = new System.Random()
                for x in 1..10 -> R.Next(-10, 10)
               ]
        
let swap f x y = f y x
let list_to_tree L = List.fold (swap insert) Nil L
let tree_to_list T = fold_infix (fun l x -> x::l) [] T

(** TREE SORTING !!! **)
let tree_sort X = list_to_tree X |> tree_to_list

tree_sort [2;3;1;4;5;4;7]

(** EXPRESSION TREES **)
type expression = | Add of expression*expression
                    | Sub of expression*expression
                    | Mult of expression*expression
                    | Div of expression*expression
                    | Value of int

let E = Add(Value(1), Mult(Value(2), Value(3)))

let rec comp = function
    | Value(x) -> x
    | Add(x,y) -> comp x + comp y
    | Sub(x,y) -> comp x - comp y
    | Mult(x,y) -> comp x * comp y
    | Div(x,y) -> comp x / comp y

comp E

(** Tail recursion in trees: CONTINUATIONS **)

(* Example *)
let rec len = function
    | [] -> 0
    | h::t -> 1+len t

let lenc L =
    let rec lenc' f = function
        | [] -> f 0
        | h::t -> lenc' (fun x-> f x+1) t
    lenc' (fun x->x) L

lenc [1;2;3;2;1]

(*************************************************)
(*************************************************)

(*** !!!!! ***)
let f1 = printfn "Hello, world"
f1;;

let f2 = function() -> printfn "Hello, world"
f2;;
f2();;
        

(*************************************************)
(*************************************************)