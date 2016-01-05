//let equalsOpt (x : #System.IEquatable<'T>) (y : #System.IEquatable<'T>) = 
let equalsOpt<'T when 'T :> System.IEquatable<'T>> (x : 'T) (y : 'T) =
    x.Equals(y)

let compareOpt (x : #System.IComparable<'T>) (y : #System.IComparable<'T>) = x.CompareTo(y)
    
let compareOpt<'T when 'T :> System.IComparable<'T>> (x : 'T) (y : 'T) = x.CompareTo(y)


[<CustomEquality; CustomComparison>]
[<Struct>]
type MStruct =
    val mutable v : int
    new (v) = { v = v }

    override x.Equals(yobj) =
        printfn "Object.Equals"
        match yobj with
        | :? MStruct as y -> equalsOpt x y
        | _ -> false
    
    override x.GetHashCode() = hash x.v
    static member op_Equality(x, y) = 
        printfn "op_Equality"
        equalsOpt x y
    static member (==) (x, y) = equalsOpt x y
    //static member (=) (x, y) = equalsOpt x y

    static member op_LessThan(x, y) = 
        printfn "op_LessThan"
        compareOpt x y
    //static member (<)(x, y) = compareOpt x y

    interface System.IEquatable<MStruct> with
        member x.Equals(y) =
            printfn "IEquatable<MStruct>.Equals"
            x.v = y.v

    interface System.IComparable with
        member x.CompareTo(yobj) =
            printfn "IComparable.CompareTo"
            match yobj with
            | :? MStruct as y -> compare x.v y.v
            | _ -> invalidArg "yobj" "cannot compare values of different types"

    interface System.IComparable<MStruct> with
        member x.CompareTo(y) =
            printfn "IComparable<MStruct>.CompareTo"
            compare x.v y.v
                    

let x = new MStruct(2)
let y = new MStruct(2)
//let x = new MStruct(2) :> System.IEquatable<_>
//let y = new MStruct(2) :> System.IEquatable<_>

compareOpt x y
equalsOpt x y
MStruct.op_Equality(x, y)   // Calls op_Equality then specialized Equatable<MStruct>
x == y                      // Calls (==) then specialized Equatable<MStruct>
x = y                       // Always calls the slower Object.Equals first instead of the custom op_Equality and then specialized IEquatable<MStruct>.Equals
x < y                       // Always calls the slower IComparable.CompareTo instead of then specialized IComparable<MStruct>.CompareTo
