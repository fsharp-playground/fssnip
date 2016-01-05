open System.Collections.Generic

type Dictionary<'K,'V> with
    member d.TryFind(k : 'K) =
        let v = ref Unchecked.defaultof<_>
        if d.TryGetValue(k, v) then Some v.Value else None


let rec Y F x = F (Y F) x

// needs nicer name!
let Y2 (F : ('Seed -> 'a -> 'b) -> 'Seed -> 'a -> 'b) =
    let dict = new Dictionary<'Seed, ('a -> 'b) ref> ()

    let F' (self : ('Seed -> 'a -> 'b)) (s : 'Seed) =
        match dict.TryFind s with
        | None ->
            let fp = ref (fun _ -> failwith "Cannot handle recursive calls in this context!")
            dict.Add(s, fp)
            let f = F self s
            fp := f
            f
        | Some f -> (fun a -> f.Value a)

    Y F'

// example : computing the maximum depth of F# DUs

open System
open System.Reflection
open Microsoft.FSharp.Reflection

let maxDepth<'T> = 
    let f = 
        Y2 (fun self (t : Type) ->
                printfn "Precomputing depth counter for %A" t
                if FSharpType.IsUnion t then
                    let reader = FSharpValue.PreComputeUnionTagReader t
                    // recursively precompute children
                    let fieldDepth (f : PropertyInfo) = let depthF = self f.PropertyType in fun o -> f.GetValue(o, [||]) |> depthF
                    let ucis = FSharpType.GetUnionCases(t) |> Seq.map(fun u -> u.Tag, u.GetFields() |> Array.map fieldDepth) |> Map.ofSeq

                    fun (o : obj) ->
                        let depths = ucis.[reader o] |> Array.map (fun f -> f o)
                        if Seq.isEmpty depths then 0 else (Seq.max depths) + 1
                else fun _ -> 0) typeof<'T>

    fun (x : 'T) -> f (x :> obj)


type Peano = Zero | Succ of Peano

let rec int2Peano = function 0 -> Zero | n -> Succ(int2Peano(n-1))

// Peano is recursive, would stack overflow if used regular Y combinator
let d = maxDepth<Peano option>

d <| Some (int2Peano 41)