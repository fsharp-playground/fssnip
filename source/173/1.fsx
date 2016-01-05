open System
open System.Reflection
open System.Reflection.Emit
open System.Runtime.InteropServices
open System.Globalization

//------------------------------------------------------------------------------
// A dynamic value whos type is unknown at runtime.
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

    member x.IsPrimitive =
      if x.IsNumber then 
        true

      else 
        match x.Tag with
        | TypeTags.String
        | TypeTags.Bool -> true
        | _ -> false

    member x.ClrBoxed =
      if x.IsNumber 
        then x.Number :> obj
        elif x.Tag = TypeTags.Bool 
          then x.Bool :> obj
          else x.Clr

    member x.Unbox<'a>() = x.ClrBoxed :?> 'a

    static member Box(value:CommonObject) =
      let mutable box = BoxedValue()
      box.Clr <- value
      box.Tag <- TypeTags.Object
      box

    static member Box(value:FunctionObject) =
      let mutable box = BoxedValue()
      box.Clr <- value
      box.Tag <- TypeTags.Function
      box

    static member Box(value:String) =
      let mutable box = BoxedValue()
      box.Clr <- value
      box.Tag <- TypeTags.String
      box

    static member Box(value:double) =
      let mutable box = BoxedValue()
      box.Number <- value
      box

    static member Box(value:bool) =
      let mutable box = BoxedValue()
      box.Number <- TaggedBools.ToTagged value
      box

    static member Box(value:Object) =
      let mutable box = BoxedValue()
      box.Clr <- value
      box.Tag <- TypeTags.Clr
      box

    static member Box(value:Object, tag:uint32) =
      let mutable box = BoxedValue()
      box.Clr <- value
      box.Tag <- tag
      box

    static member Box(value:Undefined) =
      Utils2.BoxedUndefined

  end