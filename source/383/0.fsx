namespace Ijs

open System.Runtime.InteropServices

type IEnvironment =
  interface
    abstract Undefined : unit -> IUndefined with get
    abstract Operators : unit -> IOperators with get
    abstract Globals : unit -> ICommonObject with get
  end

and ICommonObject =
  interface
    
    abstract Environment : unit -> IEnvironment with get
    abstract Put : string * BoxedValue -> unit
    abstract Get : string -> BoxedValue
    abstract Has : string -> BoxedValue
    abstract Call : ICommonObject * BoxedValue array -> BoxedValue

  end

and IUndefined =
  interface
    abstract AccessAddOperator : unit -> BoxedValue
  end

and IOperators =
  interface
    abstract Add : BoxedValue * BoxedValue -> BoxedValue
  end

and [<NoComparison>] [<StructLayout(LayoutKind.Explicit)>] BoxedValue =
  struct
    [<FieldOffset(0)>] val mutable Number : float
    [<FieldOffset(8)>] val mutable Object : obj
  end

// Imaginary file end here, and new file starts below
// notice that these types are not recursive and can be
// split over several files

type Undefined(env:IEnvironment) =
  class
    interface IUndefined with
      member x.AccessAddOperator() =
        env.Operators.Add(BoxedValue(), BoxedValue())
  end

type Operators() =
  class
    interface IOperators with
      member x.Add (left:BoxedValue, right:BoxedValue) = left
  end

type Environment(globals:ICommonObject) as x =
  
  let undefined = Undefined(x) :> IUndefined
  let operators = Operators() :> IOperators

  interface IEnvironment with
    member x.Globals = globals
    member x.Operators = operators
    member x.Undefined = undefined
