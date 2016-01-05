type ImmutableList<'T> = 
  | Empty 
  | Cons of 'T * ImmutableList<'T>

let append lst1 lst2 = 
  let rec appendCont cont = function
    | Empty -> cont lst2
    | Cons(h,t) -> appendCont (fun accList -> cont(Cons(h,accList))) t
  appendCont (fun x -> x) lst1

let res = append (Cons(1,Cons(2,Empty))) (Cons(3,Cons(4,Empty)))

(*
type ImmutableList<'T> =
| Empty
| Cons of 'T * ImmutableList<'T>
val append : ImmutableList<'a> -> ImmutableList<'a> -> ImmutableList<'a>
val res : ImmutableList<int> = Cons (1,Cons (2,Cons (3,Cons (4,Empty))))
*)