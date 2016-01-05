open System
open Microsoft.FSharp.Core.OptimizedClosures


// for 'T = 'T0 -> 'T1 -> ... -> 'Tn, returns the type of 'Tn
let codomain<'T> : 'T -> Type =
    let fsFunctionTypes =
        [
            typedefof<FSharpFunc<_,_>>
            typedefof<FSharpFunc<_,_,_>>
            typedefof<FSharpFunc<_,_,_,_>>
            typedefof<FSharpFunc<_,_,_,_,_>>
            typedefof<FSharpFunc<_,_,_,_,_,_>>
        ] 
        |> Seq.map (fun t -> t.GUID)
        |> Set.ofSeq

    let (|FSharpFunc|_|) (t : Type) =
        let isFSharpFunc (t: Type) =
            t <> null && t.IsGenericType && 
                t.GetGenericTypeDefinition().GUID |> fsFunctionTypes.Contains

        match isFSharpFunc t, isFSharpFunc t.BaseType with
        | true, _ -> Some t
        | _, true -> Some t.BaseType
        | _ -> None

    let rec traverse =
        function
        | FSharpFunc func ->
            let funcTypes = func.GetGenericArguments()
            let returningType = funcTypes.[funcTypes.Length - 1]
            traverse returningType
        | other -> other

    fun f -> f.GetType() |> traverse

// examples:
codomain 2
codomain <| fun x y z w -> (x + y + Int32.Parse(z) + w).ToString()
codomain codomain