// [snippet:Clumsy LoopBuilder]
type LoopBuilder () =
  let while' gd body = 
    (fun _ ->
      let b = gd() 
      if b then
        if Option.isSome (body ()) then Some ()
        else body () |> (fun _ -> None)
      else 
        Some ()) |> Seq.initInfinite
  member this.While(gd,body) =
      while' gd body |> Seq.tryFind (fun x -> Option.isSome x) |> ignore
  member this.For (s, f) =
      s |> Seq.tryFind (fun x -> Option.isSome (f x)) |> ignore
  member this.Zero () = None
  member this.Combine (a,b) = a |> function
    |Some x -> Some x
    |_ -> b()
  member this.Return (x) = x 
  member this.ReturnFrom (x) = Some x 
  member this.Bind (m,f) = m |> function
    |Some x -> f x |> Some
    |_ -> None
  member this.Delay f = f 
  member this.Run f = f ()

let break' = Some ()
let continue' = None
let loop = LoopBuilder ()
// [/snippet]

// [snippet:Example]
open System

printfn "%s" "----- for"
let hoge = 
  let x = ref "／(^o^)＼"
  loop {for i in [1..10] do
          if i = 5 then
            printfn "%s" "five"
            do! continue' else 
          if i = 2 then
            printfn "%s" "two"
            do! continue' else
          printfn "%d" i
          if i = 7 then
            printfn "%s" "!!!"
            x := "＼(^o^)／"
            return break'
            printfn "%d" i
          printfn "%s" "!" }
  !x
hoge |> printfn "%s"

printfn "%s" "----- while"
let fuga = 
  let x = ref "／(^o^)＼"
  loop {let i = ref 0
        while !i < 6 do
          i := !i + 1
          if !i = 5 then
            printfn "%s" "five"
            do! continue' else
          if !i = 2 then
            printfn "%s" "two"
            do! continue' else
          printfn "%d" !i
          if !i = 7 then
            printfn "%s" "!!!"
            x := "＼(^o^)／"
            return break'
            printfn "%d" !i
          printfn "%s" "!"}
  !x
fuga |> printfn "%s"
Console.ReadLine () |> ignore
// [/snippet]