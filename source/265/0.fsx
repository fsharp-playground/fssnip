
open System

///
[<AllowNullLiteral>]
type CString =
  
  ///
  val mutable Builder : Text.StringBuilder

  ///
  val mutable Length : int

  ///
  [<DefaultValue>] 
  val mutable Cached : string

  ///
  new(builder:Text.StringBuilder) = {
    Builder = builder
    Length = builder.Length
  }

  ///
  new() = {
    Builder = new Text.StringBuilder()
    Length = 0
  }

  ///
  new(initial:string) = {
    Builder = new Text.StringBuilder(initial)
    Length = initial.Length
  }

  ///
  new(left:string, right:string) = {
    Builder = (new Text.StringBuilder(left, left.Length + right.Length)).Append(right)
    Length = left.Length + right.Length
  }

  ///
  static member OfArray(values:string array) =
    let mutable length = 0

    for str in values do
      let slength = if Object.ReferenceEquals(str, null) then 0 else str.Length
      length <- length + slength
      
    let cs = CString()
    cs.Builder.EnsureCapacity(length) |> ignore

    for str in values do
      if not <| Object.ReferenceEquals(str, null) then
        cs.Builder.Append(str) |> ignore

    cs

  ///
  member x.Concat(s:string) =
    if x.Length = x.Builder.Length then
      x.Builder.Append(s) |> ignore
      CString(x.Builder)

    else
      let builder = new Text.StringBuilder(x.Builder.ToString(0, x.Length), x.Length + s.Length)
      builder.Append(s) |> ignore
      CString(builder)

  ///
  member x.Concat(cs:CString) =
    x.Concat(cs.ToString())

  ///
  member x.Concat(o:obj) =
    if o :? string then x.Concat(o :?> string)
    elif o :? CString then x.Concat(o :?> CString)
    else x.Concat(string o)

  ///
  override x.ToString() =
    if Object.ReferenceEquals(x.Cached, null) then
      x.Cached <- x.Builder.ToString(0, x.Length)

    x.Cached
