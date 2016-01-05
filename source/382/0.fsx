namespace Newtonsoft.Json.FSharp

open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open System

type TupleConverter() =
  inherit JsonConverter()

  override __.CanRead  = true
  override __.CanWrite = true
  
  override __.CanConvert(vType) = vType |> FSharpType.IsTuple

