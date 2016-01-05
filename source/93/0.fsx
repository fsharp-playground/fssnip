let inline pow b e =
    let rec loop = function
    | e when e < LanguagePrimitives.GenericOne<_> -> LanguagePrimitives.GenericOne<_>
    | e -> b * (loop (e-LanguagePrimitives.GenericOne<_>))
    loop e

(*
val inline pow :
   ^a ->  ^c ->  ^b
    when ( ^a or  ^b) : (static member ( * ) :  ^a *  ^b ->  ^b) and
          ^b : (static member get_One : ->  ^b) and
          ^c : (static member get_One : ->  ^c) and  ^c : comparison and
         ( ^c or  ^d) : (static member ( - ) :  ^c *  ^d ->  ^c) and
          ^d : (static member get_One : ->  ^d)

> pow 5 2;;
val it : int = 25
> pow 5.0 2;;
val it : float = 25.0
> *)