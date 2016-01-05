module FSharp.Monad.Continuation

open System

// [snippet: Continuation monad]
type Cont<'a,'r> =
  abstract Call : ('a -> 'r) * (exn -> 'r) -> 'r

let private protect f x cont econt =
  let res = try Choice1Of2 (f x) with err -> Choice2Of2 err
  match res with
  | Choice1Of2 v -> cont v
  | Choice2Of2 v -> econt v

let runCont (c:Cont<_,_>) cont econt = c.Call(cont, econt)
let throw exn = { new Cont<_,_> with member x.Call (cont,econt) = econt exn }
let callCC f =
  { new Cont<_,_> with
      member x.Call(cont, econt) =
        runCont (f (fun a -> { new Cont<_,_> with member x.Call(_,_) = cont a })) cont econt }
 
type ContinuationBuilder() =
  member this.Return(a) = 
    { new Cont<_,_> with member x.Call(cont, econt) = cont a }
  member this.ReturnFrom(comp:Cont<_,_>) = comp
  member this.Bind(comp1, f) = 
    { new Cont<_,_> with 
        member x.Call (cont, econt) = 
          runCont comp1 (fun a -> protect f a (fun comp2 -> runCont comp2 cont econt) econt) econt }
  member this.Catch(comp:Cont<_,_>) =
    { new Cont<Choice<_, exn>,_> with 
        member x.Call (cont, econt) = 
          runCont comp (fun v -> cont (Choice1Of2 v)) (fun err -> cont (Choice2Of2 err)) }
  member this.Zero() =
    this.Return ()
  member this.TryWith(tryBlock, catchBlock) =
    this.Bind(this.Catch tryBlock, (function Choice1Of2 v -> this.Return v 
                                           | Choice2Of2 exn -> catchBlock exn))
  member this.TryFinally(tryBlock, finallyBlock) =
    this.Bind(this.Catch tryBlock, (function Choice1Of2 v -> finallyBlock(); this.Return v 
                                           | Choice2Of2 exn -> finallyBlock(); throw exn))
  member this.Using(res:#IDisposable, body) =
    this.TryFinally(body res, (fun () -> match res with null -> () | disp -> disp.Dispose()))
  member this.Combine(comp1, comp2) = this.Bind(comp1, (fun () -> comp2))
  member this.Delay(f) = this.Bind(this.Return (), f)
  member this.While(pred, body) =
    if pred() then this.Bind(body, (fun () -> this.While(pred,body))) else this.Return ()
  member this.For(items:seq<_>, body) =
    this.Using(items.GetEnumerator(), (fun enum -> this.While((fun () -> enum.MoveNext()), this.Delay(fun () -> body enum.Current))))
let cont = ContinuationBuilder()
// [/snippet]

(* Tests *)

module FSharp.Monad.Tests.ContinuationTests

open FSharp.Monad
open FSharp.Monad.Continuation
open NUnit.Framework
open FsUnit

// [snippet: test samples]
let c n = cont { return n }
let addSomeNumbers = cont {
  let! x = c 6
  let! y = c 7
  return x + y }

[<Test>]
let ``When adding 6 to 7 and applying a continuation to convert to string and replace 1 with a, it should return a3``() =
  runCont addSomeNumbers (fun x -> x.ToString().Replace('1', 'a')) (sprintf "%A") |> should equal "a3"

[<Test>]
let ``When throwing an exception, it should catch the exception``() =
  let message = "failed"
  runCont (Continuation.throw (Exception(message))) (fun _ -> "didn't handle exception") (fun (e:exn) -> e.Message) |> should equal message

(* Test callCC *)
let sum l =
  let rec sum l = cont {
    let! result = callCC (fun exit1 -> cont {
      match l with
      | [] -> return 0
      | h::t when h = 2 -> return! exit1 42
      | h::t -> let! r = sum t
                return h + r })
    return result }
  runCont (sum l) id (fun _ -> -1)

[<Test>]
let ``When summing a list without a 2 via callCC it should return 8``() =
  sum [1;1;3;3] |> should equal 8

[<Test>]
let ``When summing a list containing 2 via callCC it should return 43``() =
  sum [1;2;3] |> should equal 43
// [/snippet]
