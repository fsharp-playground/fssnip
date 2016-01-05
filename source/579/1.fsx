open Microsoft.FSharp.Reflection

let toString (x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name

let fromString<'a> (s:string) =
    match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
    |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
    |_ -> None

// Usage:
// type A = X|Y|Z with
//     member this.toString = toString this
//     static member fromString s = fromString<A> s

// > X.toString;;
// val it : string = "X"

// > A.fromString "X";;
// val it : A option = Some X

// > A.fromString "W";;
// val it : A option = None

// > toString X;;
// val it : string = "X"

// > fromString<A> "X";;
// val it : A option = Some X
