[<RequireQualifiedAccess>]
module Array =
    [<RequireQualifiedAccess>]
    module Parallel =
        let filter predicate array =
            array |> Array.Parallel.choose (fun x -> if predicate x then Some x else None)

module NumericLiteralG =
    let inline FromZero () = LanguagePrimitives.GenericZero
    let inline FromOne () = LanguagePrimitives.GenericOne

let inline isNumPalindrome number =
    let ten =
        let two = 1G + 1G
        let three = two + 1G
        let five = three + two
        five + five
    let hundred = ten * ten

    let rec findHighDiv div =
        let div' = div * ten
        match number / div' with
        | x when x = 0G -> div
        | _             -> findHighDiv div'
    let rec impl n = function
        | x when x = 0G  -> true
        | div            -> match n / div, n % ten with
                            | a, b when a <> b -> false
                            | _                -> impl (n % div / ten) (div / hundred)
    impl number (findHighDiv 1G)

Array.Parallel.init 900 ((+) 100 >> fun n -> Array.init (1000 - n) ((+) n >> (*) n))
|> Array.concat
|> Array.Parallel.filter isNumPalindrome
|> Array.max
|> stdout.WriteLine