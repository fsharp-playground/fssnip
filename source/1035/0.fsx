namespace System.Numerics

[<AutoOpen>]
module DivRem =
    type DivRem = DivRem with
        static member (/%) (x : int32,  DivRem) = fun y -> System.Math.DivRem(x, y)
        static member (/%) (x : int64,  DivRem) = fun y -> System.Math.DivRem(x, y)
        static member (/%) (x : bigint, DivRem) = fun y -> BigInteger.DivRem(x, y)

    let inline (/%) x y = (x /% DivRem) y