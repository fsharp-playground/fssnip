type HList = interface end
and HNil = HNil with
    static member inline (|*|) (f, HNil) = f $ HNil
    static member inline Map (m, HNil) = HNil
    interface HList
and HCons<'a, 'b when 'b :> HList> = HCons of 'a * 'b with
    static member inline (|*|) (f, HCons(x, xs)) = f $ HCons(x, xs) 
    interface HList 

let inline head (HCons(h,t)) = h
let inline tail (HCons(h,t)) = t

type Peano = interface end
and Zero = Zero with
    static member inline (|*|) (f, Zero) = f $ Zero
    interface Peano
and Succ<'a when 'a :> Peano> = Succ of 'a  with
    static member inline (|*|) (f, Succ(x)) = f $ Succ(x) 
    interface Peano 

type Bool = interface end
and True = True with
    interface Bool
and False = False with
    interface Bool

let inline (^+^) head tail = HCons(head, tail)

// Examples

type Append = Append with
    static member ($) (Append, HNil) = id
    static member inline ($) (Append, HCons(x, xs)) = fun list ->
        HCons (x, (Append |*| xs) list)

type TMapper = TMapper with
    static member inline Map (s: string) = ""
    static member inline Map (i: int) = 0

type ZeroMapper = ZeroMapper with
    static member ($) (ZeroMapper, HNil) = HNil
    static member inline ($) (ZeroMapper, HCons(x : string, xs)) = HCons("", ZeroMapper |*| xs)
    static member inline ($) (ZeroMapper, HCons(x : int, xs)) = HCons(0, ZeroMapper |*| xs)

type Length = Length with
    static member ($) (Length, HNil) = Zero
    static member inline ($) (Length, HCons(x, xs)) = Succ (Length |*| xs) 


let first = 1 ^+^ '1' ^+^ HNil
let second =  "1" ^+^ true ^+^ HNil
let third = "one" ^+^ 123 ^+^ HNil

ZeroMapper $ third // HCons<string,HCons<int,HNil>> = HCons ("",HCons (0,HNil))
ZeroMapper $ second // type error, no char version of ZeroMapper defined

// result : HCons<int,HCons<char,HCons<string,HCons<bool,HNil>>>>
let result = (Append $ first) second // HCons (1,HCons ('1',HCons ("1",HCons (true,HNil))))

tail third // HCons (123,HNil)
(tail >> tail) third // HNil
(tail >> tail >> tail) third // compiler error

// length : Succ<Succ<Succ<Succ<Zero>>>>
let length = Length $ result // Succ (Succ (Succ (Succ Zero)))

