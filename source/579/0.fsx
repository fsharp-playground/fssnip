open Microsoft.FSharp.Reflection

let toString (x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name

let fromString (t:System.Type) (s:string) =
    match FSharpType.GetUnionCases t |> Array.filter (fun case -> case.Name = s) with
    |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]))
    |_ -> None

// Usage:
// type A = X|Y|Z with
//     member this.toString = toString this
//     static member fromString s = fromString typeof<A> s

// > X.toString;;
// val it : string = "X"

// > A.fromString "X";;
// val it : obj option = Some X

// > A.fromString "W";;
// val it : obj option = None

// > toString X;;
// val it : string = "X"

// > fromString typeof<A> "X";;
// val it : obj option = Some X
