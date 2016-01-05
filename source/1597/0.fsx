module Foo
open System.IO
open System.Reflection

let foo =
  use stream = File.OpenRead("foo")
  use reader = new StreamReader(stream)
  use reader2 = (stream |> new StreamReader)
  (reader, reader2)