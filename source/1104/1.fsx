let inline zero () : ^T = LanguagePrimitives.GenericZero< ^T>
let inline succ (x : ^T) = x + LanguagePrimitives.GenericOne< ^T> : ^T
let inline incr (x : ^T ref) = x := succ !x

let inline infiniteSeq (x : ^T) =
    let rec infinite x = seq { yield x ; yield! infinite (succ x) }
    infinite x


infiniteSeq 42 |> Seq.take 5 |> Seq.toArray
infiniteSeq 3.14 |> Seq.take 10 |> Seq.toArray

type Peano = Zero | Succ of Peano
with
    static member (+) (x : Peano, y : Peano) =
        let rec add x = function Zero -> x | Succ y -> add (Succ x) y
        add x y

    static member One = Succ Zero     
        
infiniteSeq Zero |> Seq.take 10 |> Seq.toArray