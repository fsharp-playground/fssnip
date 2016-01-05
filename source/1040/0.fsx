open System
open System.Reflection
open System.Runtime.Serialization

let gatherObjs (o : obj) =
    let gen = new ObjectIDGenerator()

    let rec traverse (gathered : obj list) : obj list -> obj list =
        function
        | [] -> gathered
        | o :: rest when o = null -> traverse gathered rest
        | o :: rest ->
            let firstTime = ref false
            gen.GetId(o, firstTime) |> ignore
            if firstTime.Value then
                let t = o.GetType()
                let nested =
                    if t.IsValueType then []
                    elif t.IsArray then
                        [ for e in (o :?> Array) -> e ]
                    else
                        let fields = t.GetFields(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic)
                        [ for fInfo in fields -> fInfo.GetValue o ]

                traverse (o :: gathered) (nested @ rest)

            else traverse gathered rest

    traverse [] [o]


let gatherTypes : obj -> _ =
    gatherObjs
    >> Seq.map (fun o -> o.GetType())
    >> Seq.distinctBy (fun t -> t.AssemblyQualifiedName)
    >> Seq.toList