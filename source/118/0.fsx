let rec fix f x = f (fix f) x

let inline factf1 n =
    let loop = fix (fun f x -> 
        if x<=LanguagePrimitives.GenericOne 
            then LanguagePrimitives.GenericOne
            else x * (f (x - LanguagePrimitives.GenericOne)))
    loop n

let rec fix2 f a b = f (fix2 f) a b

let inline factf2 n =
    let loop = fix2 (fun f acc n -> 
        if n<=LanguagePrimitives.GenericOne 
            then acc 
            else f (n*acc) (n-LanguagePrimitives.GenericOne))
    loop LanguagePrimitives.GenericOne n

let inline facti n =
    let rec loop acc n = 
       if n<=LanguagePrimitives.GenericOne 
        then acc 
        else loop (n*acc) (n-LanguagePrimitives.GenericOne)
    loop LanguagePrimitives.GenericOne n


(* It seems there's little difference performance-wise. Still, tail-recursive implementation is faster than CPS.
> [1I..2000I] |> List.iter (factf1 >> ignore);;
Real: 00:00:04.603, CPU: 00:00:04.586, GC gen0: 480, gen1: 1, gen2: 0
val it : unit = ()
> [1I..2000I] |> List.iter (factf2 >> ignore);;
Real: 00:00:04.868, CPU: 00:00:04.851, GC gen0: 556, gen1: 1, gen2: 0
val it : unit = ()
> [1I..2000I] |> List.iter (facti >> ignore);;
Real: 00:00:04.043, CPU: 00:00:04.024, GC gen0: 548, gen1: 0, gen2: 0
val it : unit = ()
> 
*)
