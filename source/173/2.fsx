#nowarn "9"

open System
open System.Reflection
open System.Reflection.Emit
open System.Runtime.InteropServices
open System.Globalization

module TypeTags =
  let [<Literal>] Box = 0x00000000u
  let [<Literal>] Bool = 0xFFFFFF01u
  let [<Literal>] Number = 0xFFFFFF02u
  let [<Literal>] Clr = 0xFFFFFF03u
  let [<Literal>] String = 0xFFFFFF04u
  let [<Literal>] Undefined = 0xFFFFFF05u
  let [<Literal>] Object = 0xFFFFFF06u
  let [<Literal>] Function = 0xFFFFFF07u

  let Names = 
    Map.ofList [
      (Box, "internal")
      (Bool, "boolean")
      (Number, "number")
      (Clr, "clr")
      (String, "string")
      (Undefined, "undefined")
      (Object, "object")
      (Function, "function")]

  let getName (tag:uint32) = Names.[tag]

module Markers =
  let [<Literal>] Number = 0xFFF8us
  let [<Literal>] Tagged = 0xFFF9us

type CommonObject = class end
type FunctionObject = class end
type ArrayObject = class end

(* A dynamic value whose type is unknown at runtime. *)
type [<NoComparison>] [<StructLayout(LayoutKind.Explicit)>] BoxedValue =
  struct 
    //Reference Types
    [<FieldOffset(0)>] val mutable Clr : Object 
    [<FieldOffset(0)>] val mutable Object : CommonObject
    [<FieldOffset(0)>] val mutable Array : ArrayObject
    [<FieldOffset(0)>] val mutable Func : FunctionObject
    [<FieldOffset(0)>] val mutable String : String
    [<FieldOffset(0)>] val mutable Scope : BoxedValue array

    //Value Types
    [<FieldOffset(8)>] val mutable Bool : bool
    [<FieldOffset(8)>] val mutable Number : double

    //Type & Tag
    [<FieldOffset(12)>] val mutable Tag : uint32
    [<FieldOffset(14)>] val mutable Marker : uint16

    member x.IsNumber = x.Marker < Markers.Tagged
    member x.IsTagged = x.Marker > Markers.Number
    member x.IsString = x.IsTagged && x.Tag = TypeTags.String
    member x.IsObject = x.IsTagged && x.Tag >= TypeTags.Object
    member x.IsFunction = x.IsTagged && x.Tag >= TypeTags.Function
    member x.IsBoolean = x.IsTagged && x.Tag = TypeTags.Bool
    member x.IsUndefined = x.IsTagged && x.Tag = TypeTags.Undefined
    member x.IsClr = x.IsTagged && x.Tag = TypeTags.Clr
  end