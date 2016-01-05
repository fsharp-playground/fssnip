let printBits n =
    Array.init 34 (function
        | 0 -> '0'
        | 1 -> 'b'
        | i when n >>> 33 - i &&& 1 = 1 -> '1'
        | _ -> '0'
    )
    |> fun cs -> System.String cs

0b0010010011111
|> fun x -> int (printBits x) = x

-19
|> fun x -> int (printBits x) = x