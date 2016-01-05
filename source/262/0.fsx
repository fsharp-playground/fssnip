/// Simple implementation of the rope data structure
/// http://en.wikipedia.org/wiki/Rope_(computer_science) 
/// 
/// Uses an F# Map<int, string> for internal storage
module Rope =

  open System
  open System.Text

  type T = {
    Value: Map<int, string>
    Low: int
    High: int
    Length: int
  }

  type Rope = T
  
  let empty =
    {Value=Map.empty; Low=0; High=0; Length=0}

  let append s (t:T) =
    {t with 
      Value=t.Value.Add(t.High, s)
      High=t.High+1
      Length=t.Length+s.Length
    }

  let prepend s (t:T) =
    {t with 
      Value=t.Value.Add(t.Low-1, s)
      Low=t.Low-1
      Length=t.Length+s.Length
    }

  let toString (t:T) =
    let buffer = new StringBuilder(t.Length)
    for kvp in t.Value do
      buffer.Append(kvp.Value) |> ignore
    buffer.ToString()

/// Real: 00:00:00.033, CPU: 00:00:00.031, GC gen0: 1, gen1: 0, gen2: 0
let mutable r = Rope.empty
for i = 0 to 10000 do
  r <- r |> Rope.append "foo"

/// Real: 00:00:00.344, CPU: 00:00:00.358, GC gen0: 282, gen1: 1, gen2: 1
let mutable s = ""
for i = 0 to 10000 do
  s <- s + "foo"