let rec sum f n = 
  if n = 0 then 0 else f n + sum f (n-1)

let limit   = 100
let memopad : int option array = Array.init 100 (fun x -> None)

let rec p' n = 
  if n = 1 then 1 else sum (fun k -> (p k) * (p (n-k)) ) (n-1) 

and p n = 
  if n < limit then
    match memopad.[n] with
    | Some r -> r
    | None   -> let r = p' n in memopad.[n] <- Some r; r
  else p' n

module Susp =
  exception Impossible
  exception Circular

  type 'a susp = unit -> 'a
  let force t  = t ()
  let delay (t : 'a susp) = 
      
    let memo : 'a susp ref = ref (fun () -> raise Impossible)
    let t' () = 
      let r = t () in memo := (fun () -> r); r
    in memo := t'
    fun () -> (!memo)()

  (* implement loopback using 'backpatching' *)
  let loopback f = 
    let r = ref (fun () -> raise Circular)
    let t = fun () -> (!r)() in r := f t; t

let t = Susp.delay (fun () -> printfn "hello")
Susp.force t (* prints hello *)
Susp.force t (* silent *)

type 'a stream_ = Cons of 'a * 'a stream 
and 'a stream   = 'a stream_ Susp.susp

let ones_loop s = Susp.delay (fun () -> Cons (1, s))
let ones = Susp.loopback ones_loop