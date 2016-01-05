type HList = interface end
and HNil = HNil with
    static member inline (|*|) (f, HNil) = f $ HNil
    interface HList
and HCons<'a, 'b when 'b :> HList> = HCons of 'a * 'b with
    static member inline (|*|) (f, HCons(x, xs)) = f $ HCons(x, xs) 
    interface HList 


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
let first = 1 ^+^ '1' ^+^ HNil
let second =  "1" ^+^ true ^+^ HNil
let third = "one" ^+^ 123 ^+^ HNil

let inline head (HCons(h,t)) = h
let inline tail (HCons(h,t)) = t

tail third // HCons (123,HNil)
(tail >> tail) third // HNil
(tail >> tail >> tail) third // compiler error

type Append = Append with
    static member ($) (Append, HNil) = id
    static member inline ($) (Append, HCons(x, xs)) = fun list ->
        HCons (x, (Append |*| xs) list)

let result = (Append $ first) second // HCons (1,HCons ('1',HCons ("1",HCons (true,HNil))))

type Length = Length with
    static member ($) (Length, HNil) = Zero
    static member inline ($) (Length, HCons(x, xs)) = Succ (Length |*| xs) 

let length = Length $ result // Succ (Succ (Succ (Succ Zero)))

type Count = Count with
    static member ($) (Count, HNil) = 0
    static member inline ($) (Count, HCons(x, xs)) = 1 + (Count |*| xs) 

let count = Count $ result // 4

// Mapper Construct for HList
type Mapper<'a> = Mapper of 'a with
    static member ($) (Mapper(M), HNil) = HNil
    static member inline ($) (Mapper(M), HCons(x, xs)) = HCons(M $ x, (Mapper(M) |*| (xs)))

// Map Example
type ZeroMap = ZeroMap with
    static member inline ($) (ZeroMap, s: string) = ""
    static member inline ($) (ZeroMap, i: int) = 0

Mapper(ZeroMap) $ third //HCons<string,HCons<int,HNil>> = HCons ("",HCons (0,HNil))
Mapper(ZeroMap) $ second // Compiler error because bool isn't defined on ZeroMap

let ZeroMapper = Mapper(ZeroMap) 
ZeroMapper $ third //HCons<string,HCons<int,HNil>> = HCons ("",HCons (0,HNil))

// Folder Construct for HList
type Folder<'a, 'v> = Folder of 'a * 'v with
    static member ($) (Folder(F, v), HNil) = v
    static member inline ($) (Folder(F, v), HCons(x, xs)) = Folder(F, F $ (v,x)) |*| xs

// Example of converting to strings and folding them up
type StrFolder = StrFolder with
    static member inline ($) (StrFolder, (s1: string, s2: string)) = s1 + s2 
    static member inline ($) (StrFolder, (s1: string, i2: int)) = s1 + (string i2)

Folder(StrFolder, "") $ third // "one123"
Folder(StrFolder, "") $ second // Compiler error because string * bool is not defined on StrFolder

// Example of reversing and HList with a Folder
type RevFolder = RevFolder with
    static member inline ($) (RevFolder, (HNil, v)) = HCons(v, HNil)
    static member inline ($) (RevFolder, (HCons(x, xs), v)) = HCons(v, HCons(x, xs))

Folder(RevFolder, HNil) $ third // HCons (123,HCons ("one",HNil))



