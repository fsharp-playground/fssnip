/// Simple implementation of the rope data structure
/// http://en.wikipedia.org/wiki/Rope_(computer_science) 
/// 
/// Uses an F# Map<int, string> for internal storage
module Rope =

  open System
  open System.Text

  type T = {
    Value: string array
    Index: int
    Length: int
  }

  type Rope = T
  
  let empty() =
    {Value=Array.zeroCreate 200; Index=0; Length=0}

  let append s (t:T) =
    t.Value.[t.Index] <- s
    {t with 
      Index=t.Index+1
      Length=t.Length+s.Length
    }

  let toString (t:T) =
    let buffer = new StringBuilder(t.Length)
    for i = 0 to t.Index-1 do
      buffer.Append(t.Value.[i]) |> ignore
    buffer.ToString()

/// Real: 00:00:00.045, CPU: 00:00:00.046, GC gen0: 27, gen1: 0, gen2: 0
let mutable r = Rope.empty()
for i = 0 to 10000 do
  r <- Rope.empty()
  for i = 0 to 100 do
    r <- r |> Rope.append "foo"

/// Real: 00:00:00.359, CPU: 00:00:00.358, GC gen0: 308, gen1: 0, gen2: 0
let mutable s = ""
for i = 0 to 10000 do
  s <- ""
  for i = 0 to 100 do
    s <- s + "foo"