open System

type ProjectionComparison<'Id, 'Cmp when 'Cmp : comparison> (token : 'Cmp) =
    member __.ComparisonToken = token

    interface IComparable with
        member x.CompareTo y =
            match y with
            | :? ProjectionComparison<'Id, 'Cmp> as y -> compare token y.ComparisonToken
            | _ -> invalidArg "y" "invalid comparand."

    override x.Equals y =
        match y with
        | :? ProjectionComparison<'Id, 'Cmp> as y -> token = y.ComparisonToken
        | _ -> false

    override x.GetHashCode() = hash token

// example

type ComparableType(t : Type) =
    inherit ProjectionComparison<ComparableType, string>(t.AssemblyQualifiedName)
    member __.Type = t


ComparableType(typeof<int>) < ComparableType(typeof<int>)