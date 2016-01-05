open System.Numerics
// optional
open MathNet.Numerics

[<AutoOpen>]
module NumericLiteralG = 
    type GenericNumber = GenericNumber with
        static member inline genericNumber (x:int32, _:int8) = int8 x
        static member inline genericNumber (x:int32, _:uint8) = uint8 x
        static member inline genericNumber (x:int32, _:int16) = int16 x
        static member inline genericNumber (x:int32, _:uint16) = uint16 x
        static member inline genericNumber (x:int32, _:int32) = x
        static member inline genericNumber (x:int32, _:uint32) = uint32 x
        static member inline genericNumber (x:int32, _:int64) = int64 x
        static member inline genericNumber (x:int32, _:uint64) = uint64 x
        static member inline genericNumber (x:int32, _:float32) = float32 x
        static member inline genericNumber (x:int32, _:float) = float x
        static member inline genericNumber (x:int32, _:bigint) = bigint x
        static member inline genericNumber (x:int32, _:decimal) = decimal x
        static member inline genericNumber (x:int32, _:Complex) = Complex.op_Implicit x
        static member inline genericNumber (x:int64, _:int64) = int64 x
        static member inline genericNumber (x:int64, _:uint64) = uint64 x
        static member inline genericNumber (x:int64, _:float32) = float32 x
        static member inline genericNumber (x:int64, _:float) = float x
        static member inline genericNumber (x:int64, _:bigint) = bigint x
        static member inline genericNumber (x:int64, _:decimal) = decimal x
        static member inline genericNumber (x:int64, _:Complex) = Complex.op_Implicit x
        static member inline genericNumber (x:string, _:float32) = float32 x
        static member inline genericNumber (x:string, _:float) = float x
        static member inline genericNumber (x:string, _:bigint) = bigint.Parse x
        static member inline genericNumber (x:string, _:decimal) = decimal x
        static member inline genericNumber (x:string, _:Complex) = Complex(float x, 0.0)
        // MathNet.Numerics
        static member inline genericNumber (x:int32, _:Complex32) = Complex32.op_Implicit x
        static member inline genericNumber (x:int32, _:bignum) = bignum.FromInt x
        static member inline genericNumber (x:int64, _:Complex32) = Complex32.op_Implicit x
        static member inline genericNumber (x:int64, _:bignum) = bignum.FromBigInt (bigint x)
        static member inline genericNumber (x:string, _:Complex32) = Complex32(float32 x, 0.0f)
        static member inline genericNumber (x:string, _:bignum) = bignum.FromBigInt (bigint.Parse x)

    let inline instance (a: ^a, b: ^b, c: ^c) = ((^a or ^b or ^c) : (static member genericNumber: ^b * ^c -> ^c) (b, c))
    let inline genericNumber num = instance (GenericNumber, num, Unchecked.defaultof<'b>)

    let inline FromZero () = LanguagePrimitives.GenericZero
    let inline FromOne () = LanguagePrimitives.GenericOne
    let inline FromInt32 n = genericNumber n
    let inline FromInt64 n = genericNumber n
    let inline FromString n = genericNumber n
