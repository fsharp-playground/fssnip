[<RequireQualifiedAccess>]
module Enum =

  let isDefined<'a, 'b when 'a : enum<'b>> (value:'a) =
    let (!<) = box >> unbox >> uint64
    let typ = typeof<'a>
    if typ.IsDefined(typeof<System.FlagsAttribute>, false) then
      ((!< value, System.Enum.GetValues(typ) |> unbox)
      ||> Array.fold (fun n v -> n &&& ~~~(!< v)) = 0UL)
    else System.Enum.IsDefined(typ, value)
