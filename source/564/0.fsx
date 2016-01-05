// [snippet: Semi-Coroutine]
open FSharpx.Continuation

type Fiber<'T>(f : 'T -> Cont<'T, 'T>) =
  let alive : bool ref = ref true
  let cont : ('T -> Cont<'T, 'T>) ref = ref <| fun x -> cont {
    let! result = f x
    alive := false
    return result
  }
  member this.Yield(x : 'T) : Cont<'T, 'T> =
    callcc <| fun exit ->
      let c = !cont
      cont := exit
      c x
  member this.Resume(x : 'T) : 'T = 
    this.Yield(x) id raise
  member this.IsAlive : bool = !alive
// [/snippet]
// [snippet: Examples]
let rec hw = new Fiber<string>(fun first -> cont {
  let! second = hw.Yield(first + "!")
  return first + ", " + second + "!"
})

hw.IsAlive
|> printfn "%b"

hw.Resume("Hello")
|> printfn "%s"

hw.IsAlive
|> printfn "%b"

hw.Resume("World")
|> printfn "%s" 

hw.IsAlive
|> printfn "%b"


let rec fib = new Fiber<int>(fun _ -> cont {
  let a, b = ref 0, ref 1
  while true do
    let c = !a
    a := !b
    b := c + !b
    let! _ = fib.Yield(!a)
    return ()
  return failwith "never reach"
})

for _ in 1..10 do
  fib.Resume(0)
  |> printfn "%d"
// [/snippet]