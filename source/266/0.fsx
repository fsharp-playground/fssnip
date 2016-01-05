namespace IronJS

open System
open System.Text

///
type [<AllowNullLiteral>] SuffixString() =
  
  [<DefaultValue>] val mutable Builder : StringBuilder
  [<DefaultValue>] val mutable Length : int
  [<DefaultValue>] val mutable Cached : string

  ///
  override x.ToString() =
    if Object.ReferenceEquals(x.Cached, null) then
      x.Cached <- x.Builder.ToString(0, x.Length)

    x.Cached

  ///
  static member Concat(suffix:SuffixString, o:obj) =
    let s = o.ToString()

    let builder = 
      if suffix.Length = suffix.Builder.Length then 
        suffix.Builder.Append(s)

      else 
        let oldValue = suffix.Builder.ToString(0, suffix.Length)
        let newLength = suffix.Length + s.Length
        (new StringBuilder(oldValue, newLength)).Append(s)

    let suffix = SuffixString()
    suffix.Builder <- builder
    suffix.Length <- builder.Length
    suffix

  ///
  static member Concat(left:obj, right:obj) =
    let left = left.ToString()
    let right = right.ToString()
    let suffix = SuffixString()
    suffix.Builder <- new StringBuilder(left, left.Length + right.Length)
    suffix.Builder.Append(right) |> ignore
    suffix.Length <- suffix.Builder.Length
    suffix

  ///
  static member OfArray(values:string array) =
    let cs = SuffixString()

    for str in values do
      let slength = if Object.ReferenceEquals(str, null) then 0 else str.Length
      cs.Length <- cs.Length + slength
      
    cs.Builder <- new StringBuilder(cs.Length)

    for str in values do
      if not <| Object.ReferenceEquals(str, null) then
        cs.Builder.Append(str) |> ignore

    cs
