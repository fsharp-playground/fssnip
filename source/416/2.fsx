module FSharp.Collections.JoinList

open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts
// [snippet: Join List]
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
            yield! x :> seq<'a>
            yield! y :> seq<'a> }
      enumerable.GetEnumerator() :> IEnumerator

module JoinList =
  let empty<'a> : JoinList<'a> = Empty
  let isEmpty l = match l with Empty -> true | _ -> false
  let length l =
    match l with
    | Empty -> 0 
    | Unit _ -> 1
    | Join(_,_,l) -> l
  let singleton x = Unit x
  let ofSeq s = Seq.fold (fun xs x ->
    match xs with 
    | Empty -> Unit x
    | Unit _ -> Join(xs, Unit x, 2)
    | Join(_,_,l) -> Join(xs, Unit x, l+1)) Empty s
  let toSeq (l:JoinList<_>) = l :> seq<_>
  let toList (l:JoinList<_>) = List.ofSeq l   // NOTE: There is likely a better conversion to the List type.
  let toArray (l:JoinList<_>) = Array.ofSeq l // NOTE: There is likely a better conversion to the Array type.
  let rec equal left right =
    match left with
    | Empty -> match right with Empty -> true | _ -> false
    | Unit x -> match right with Unit y -> x = y | _ -> false
    | Join(x,y,l) ->
      match right with
      | Join(x',y',l') -> l = l' && equal x x' && equal y y' // TODO: || iterate each and compare the values.
      | _ -> false 
  let cons hd tl =
    match tl with
    | Empty -> Unit hd
    | _ -> Join(Unit hd, tl, length tl + 1)
  let append left right =
    match left with
    | Empty -> right
    | _ -> match right with
           | Empty -> left
           | _ -> Join(left, right, length left + length right)
  let rec head l =
    match l with
    | Unit x -> x
    | Join(x,y,_) -> head x
    | _ -> failwith "JoinList.head: empty list"
  let tail (l:JoinList<'a>) : JoinList<'a> =
    let rec step (xs:JoinList<'a>) (acc:JoinList<'a>) : JoinList<'a> =
      match xs with
      | Empty -> acc
      | Unit _ -> acc
      | Join(x,y,_) -> step x (append y acc)
    if isEmpty l then Empty else step l Empty
        
type JoinList<'a> with
  static member op_Equality(left, right) = JoinList.equal left right
  static member op_Nil() = JoinList.empty
  static member op_Cons(hd, tl) = JoinList.cons hd tl
  static member op_Append(left, right) = JoinList.append left right
// [/snippet]

module FSharp.Collections.Tests.JoinListTest

open System
open FSharp.Collections.JoinList
open FSharp.Collections.JoinList.JoinList
open NUnit.Framework
open FsUnit
// [snippet: Tests]
[<Test>]
let ``test should verify empty is Empty``() =
  empty<_> |> should equal JoinList.Empty

let expected = Join(Unit 0, Join(Unit 1, Join(Unit 2, Join(Unit 3, Unit 4, 2), 3), 4), 5)

[<Test>]
let ``test length should return 5``() =
  length expected |> should equal 5

[<Test>]
let ``test ofSeq should create a JoinList from a seq``() =
  let test = seq { for i in 0..4 -> i }
  JoinList.ofSeq test |> should equal expected

[<Test>]
let ``test ofSeq should create a JoinList from a list``() =
  let test = [ for i in 0..4 -> i ]
  JoinList.ofSeq test |> should equal expected

[<Test>]
let ``test ofSeq should create a JoinList from an array``() =
  let test = [| for i in 0..4 -> i |]
  JoinList.ofSeq test |> should equal expected

[<Test>]
let ``test cons should prepend 10 to the front of the original list``() =
  cons 10 expected |> should equal (Join(Unit 10, expected, 6))

[<Test>]
let ``test singleton should return a Unit containing the solo value``() =
  singleton 1 |> should equal (Unit 1)

[<Test>]
let ``test cons should return a Unit when the tail is Empty``() =
  cons 1 JoinList.empty |> should equal (Unit 1)

[<Test>]
let ``test subsequent cons should create a JoinList just as the constructor functions``() =
  cons 0 (cons 1 (cons 2 (cons 3 (cons 4 empty)))) |> should equal expected

[<Test>]
let ``test append should join two JoinLists together``() =
  append expected expected |> should equal (Join(expected, expected, 10))

[<Test>]
let ``test head should return the first item in the JoinList``() =
  head (append expected expected) |> should equal 0

[<Test>]
let ``test tail should return all items except the head``() =
  tail (append expected expected) |> should equal (Join(cons 1 (cons 2 (cons 3 (cons 4 empty))), expected, 9))

[<Test>]
let ``test JoinList should respond to Seq functions such as map``() =
  let testmap x = x*x
  let actual = Seq.map testmap (append expected expected)
  let expected = seq { yield 0; yield 1; yield 2; yield 3; yield 4; yield 0; yield 1; yield 2; yield 3; yield 4 } |> Seq.map testmap
  Assert.That(actual, Is.EquivalentTo expected) 
// [/snippet]