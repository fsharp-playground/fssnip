open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

type JoinList<'a> =
  | Empty
  | Unit of 'a
  | Join of JoinList<'a> * JoinList<'a> * int (* the total length of the JoinList *)
  with
  interface IEnumerable<'a> with
    member this.GetEnumerator() =
      let enumerable = seq {
        match this with
        | Empty -> () 
        | Unit x -> yield x
        | Join(x,y,_) ->
            yield! x :> seq<'a>
            yield! y :> seq<'a> }
      enumerable.GetEnumerator()
    member this.GetEnumerator() =
      let enumerable = seq {
        match this with
        | Empty -> () 
        | Unit x -> yield x
        | Join(x,y,_) ->
            yield! x :> IEnumerable<'a>
            yield! y :> IEnumerable<'a> }
      enumerable.GetEnumerator() :> IEnumerator

module JoinList =
  let empty<'a> : JoinList<'a> = Empty
  let isEmpty l = match l with Empty -> true | _ -> false
  let length l = match l with
                 | Empty -> 0
                 | Unit _ -> 1
                 | Join(_,_,l) -> l
  let singleton x = Unit x
  let ofSeq s = Seq.fold (fun xs x ->
    match xs with
    | Join(_,_,l) -> Join(xs, Unit x, l+1)
    | _ -> Join(Unit x, Empty, 1)) Empty s
  let toSeq (l:JoinList<_>) = l :> seq<_>
  let toList (l:JoinList<_>) = List.ofSeq l
  let toArray (l:JoinList<_>) = Array.ofSeq l
  let rec equal left right =
    match left with
    | Empty -> match right with Empty -> true | _ -> false
    | Unit x -> match right with Unit y -> x = y | _ -> false
    | Join(x,y,l) ->
      match right with
      | Join(x',y',l') -> l = l' && equal x x' && equal y y'
                          // TODO: || iterate each and compare the values.
      | _ -> false 
  let cons hd tl = Join(Unit hd, tl, length tl + 1)
  let append left right = Join(left, right, length left + length right)
  let rec head l =
    match l with
    | Unit x -> x
    | Join(x,y,_) -> head x
    | _ -> failwith "JoinList.head: empty list"
  let tail (l:JoinList<'a>) : JoinList<'a> =
    let rec step (xs:JoinList<'a>) (acc:JoinList<'a>) : JoinList<'a> =
      match xs with
      | Empty -> Empty
      | Unit _ -> acc
      | Join(x,y,_) -> step x (append y acc)
    step l Empty
        
type JoinList<'a> with
  static member op_Equality(left, right) = JoinList.equal left right
  static member op_Nil() = JoinList.empty
  static member op_Cons(hd, tl) = JoinList.cons hd tl
  static member op_Append(left, right) = JoinList.append left right
