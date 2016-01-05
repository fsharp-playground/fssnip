// Learn more about F# at http://fsharp.net

#nowarn "40"

open System
open System.IO
open System.Collections.Generic

let parseToTuple (line : string) =
    let parsedLine = line.Split(' ') |> Array.filter(not << String.IsNullOrWhiteSpace) |> Array.map Int32.Parse
    (parsedLine.[0], parsedLine.[1])

let memoize f =
    let cache = Dictionary<_, _>()
    fun x ->
        if cache.ContainsKey(x)
            then cache.[x]
        else
            let res = f x
            cache.[x] <- res
            res

type Item (parsedLine : int * int) =
    member this.Value = fst parsedLine
    member this.Size  = snd parsedLine

type ContinuationBuilder() = 
    member b.Bind(x, f) = fun k -> x (fun x -> f x k)
    member b.Return x = fun k ->  k x
    member b.ReturnFrom x = x

let cont = ContinuationBuilder()

[<EntryPoint>]
let main args =
    let header, items = File.ReadLines(args.[0]) 
                        |> Seq.filter (fun (x : string) -> not <| String.IsNullOrEmpty x)
                        |> Seq.map parseToTuple 
                        |> Seq.toList
                        |> function
                           | h::t -> h, t
                           | _    -> raise (Exception("Wrong data format"))

    let N, K = header
    printfn "N = %d, K = %d" N K
    let items = List.map (fun x -> Item x) items |> Array.ofList
    
    let rec combinations =
        let innerSolver key =
            cont
                {
                    match key with
                    | (i, k) when i = 0 || k = 0        -> return 0
                    | (i, k) when items.[i-1].Size > k  -> return! combinations (i-1, k)
                    | (i, k)                            -> let item = items.[i-1]
                                                           let! v1 = combinations (i-1, k)
                                                           let! beforeItem = combinations (i-1, k-item.Size)
                                                           let v2 = beforeItem + item.Value
                                                           return max v1 v2
                }
        memoize innerSolver

    let res = combinations (N, K) id
    printfn "%d 0" res

    let output = 
        List.rev
            [
                let i = ref N
                let k = ref K
                for item in Array.rev items do
                    match !k - item.Size with
                    | x when x <  0 -> i := !i - 1
                                       yield 0
                    | x when x >= 0 -> let v1 = combinations (!i-1, !k) id
                                       let vc = combinations (!i, !k) id
                                       if v1 = vc then 
                                        i := !i - 1
                                        yield 0
                                       else
                                        i := !i - 1
                                        k := x
                                        yield 1
            ]

    List.iter (fun x -> printf "%A " x) output

    0