module NumericLiteralG =
    let inline FromZero () = LanguagePrimitives.GenericZero
    let inline FromOne () = LanguagePrimitives.GenericOne

let inline isqrt num =
    if num > 0G then
        let two = 1G + 1G
        let inline reduce n = (num / n + n) / two
        let rec impl n = function
            | n' when n' <= n -> n'
            | _               -> impl (reduce n) n
        let n = num / two + 1G
        impl (reduce n) n
    elif num = 0G then num
    else invalidArg "num" "negative numbers are not supported"