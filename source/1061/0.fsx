open System

[<AbstractClass>]
type ProjectionComparison<'Id, 'Cmp when 'Cmp : comparison> () =
    abstract ComparisonToken : 'Cmp

    interface IComparable with
        member x.CompareTo y =
            match y with
            | :? ProjectionComparison<'Id, 'Cmp> as y -> compare x.ComparisonToken y.ComparisonToken
            | _ -> invalidArg "y" "invalid comparand."

    override x.Equals y =
        match y with
        | :? ProjectionComparison<'Id, 'Cmp> as y -> compare x.ComparisonToken y.ComparisonToken = 0
        | _ -> false

    override x.GetHashCode() = x.ComparisonToken.GetHashCode()

// example

type ComparableType(t : Type) =
    inherit ProjectionComparison<ComparableType, string>()
    override __.ComparisonToken = t.AssemblyQualifiedName
    member __.Type = t


ComparableType(typeof<int>) < ComparableType(typeof<int>)