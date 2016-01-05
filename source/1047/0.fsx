open System
open Microsoft.FSharp.Reflection


let createArgParser<'T> =
    let primParsers =
        dict [
            typeof<int>, fun x -> Int32.Parse x :> obj
            typeof<string>, fun x -> x :> obj
            typeof<bool>, fun x -> Boolean.Parse x :> obj
        ]

    let preComputeCaseInfo (uci : UnionCaseInfo) =
        let name = "--" + uci.Name.ToLower().Replace('_', '-')
        let fieldParsers = 
            uci.GetFields() 
            |> Array.map (
                fun f ->
                    if primParsers.ContainsKey f.PropertyType then primParsers.[f.PropertyType] 
                    else failwith "unsupported field." )

        name, (uci, fieldParsers)

    let idx = FSharpType.GetUnionCases typeof<'T> |> Seq.map preComputeCaseInfo |> Map.ofSeq

    let parseArg (pos : int ref, args : string []) =
        match idx.TryFind (args.[!pos]) with
        | None -> failwithf "invalid argument %s" args.[!pos]
        | Some (uci, fieldParsers) ->
            incr pos
            let fields = 
                [| 
                    for fp in fieldParsers do
                        yield fp args.[!pos]
                        incr pos
                |]

            FSharpValue.MakeUnion(uci, fields) :?> 'T

    let parse (args : string []) =
        let pos = ref 0
        [
            while !pos < args.Length do
                yield parseArg (pos, args)
        ]

    parse

// sample template

type CLArgs =
    | Host of string
    | Port of int
    | Working_Directory of string
    | Detach


let argP = createArgParser<CLArgs>

let dummy = [| "--port" ; "12" ; "--working-directory" ; "C:/temp" ; "--host" ; "localhost" ; "--port" ; "13" ; "--detach" |]

argP dummy