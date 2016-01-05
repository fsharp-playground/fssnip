[<AutoOpen>]
module Interop =

    let (===) a b = obj.ReferenceEquals(a, b)
    let (<=>) a b = not (a === b)
    let inline isNull value = value === null
    let inline nil<'T> = Unchecked.defaultof<'T>
    let inline safeUnbox value = if isNull value then nil else unbox value
    let (|Null|_|) value = if isNull value then Some() else None
