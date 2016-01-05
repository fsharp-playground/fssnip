open System
open System.Text

(*[omit:(Performance measurement instrumentation)]*)
let time f x =
    let t = new System.Diagnostics.Stopwatch()
    t.Start()
    try
        f x
    finally
        printf "Took %dms\n" t.ElapsedMilliseconds(*[/omit]*)

// Glue up using Seq.collect (worst peformer)
let glueUp1 len ss =
    new string (
        ss
        |> Seq.collect (fun s -> Seq.ofArray <| s.ToString().ToCharArray())
        |> Seq.take len
        |> Seq.toArray)

// Glue up using Seq.scan + Seq.skipWhile
let glueUp2 len ss =
    ss
    |> Seq.scan (fun (accum: StringBuilder) s -> accum.Append(s.ToString())) (StringBuilder())
    |> Seq.skipWhile (fun x -> x.Length < len)
    |> Seq.head |> string |> fun x -> x.Substring(0, len)

// Glue up using Seq.pick and a closure over StringBuilder (best performer)
let glueUp3 len ss =
    let glue len =
        let accum = StringBuilder()
        fun item ->
            if accum.Length >= len then
                Some(accum.ToString())
            else
                accum.Append(item.ToString()) |> ignore
                None
    
    ss
    |> Seq.pick(glue len) |> fun x -> x.Substring(0, len)

(*[omit:(Performance measurement)]*)
time (glueUp1 1000000) (Seq.initInfinite(fun x -> x)) |> ignore
time (glueUp2 1000000) (Seq.initInfinite(fun x -> x)) |> ignore
time (glueUp3 1000000) (Seq.initInfinite(fun x -> x)) |> ignore(*[/omit]*)

(*[omit:(Project Euler Problem 40 Solution)]*)
// Project Euler Problem 40
let digits = Seq.initInfinite (fun n -> n) |> glueUp2 1000001
[1;10;100;1000;10000;100000;1000000]
|> List.fold (fun result x -> result * Int32.Parse(string digits.[x])) 1
|> printfn "Project Euler Problem 40 Answer: %d"(*[/omit]*)
