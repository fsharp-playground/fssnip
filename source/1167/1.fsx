namespace Talk.Functional

open System
open System.Threading
open System.Diagnostics
open System.Collections.Generic

(* PATTERN MATCHING *)

module Example1_functional =
    let message =
        match Console.ReadKey().KeyChar with
        |'Q' -> "Quit!"
        |'?' -> "Help!"
        |key -> sprintf "You hit %c" key

    printfn "%s" message


module Example2_functional =
    let processArray arr =
        match arr with
        | [| 42 |] -> "1 element, 42"
        | [| x; y; _ |] when x < y -> sprintf "%d < %d" x y
        | [| _; _; z |] ->  sprintf "3 elements, last is %d" z
        | _ ->  sprintf "%A" arr


(* ACTIVE PATTERNS *)

module Example3_functional = 

    let (|Negative|Zero|Positive|) n =
        if n < 0 then Negative
        elif n = 0 then Zero
        else Positive

    let (|Prime|Composite|Neither|) n =
        match n with
        | Zero
        | 1
        | Negative ->
            Neither
        | Positive ->
            let mutable factors = []
            let mutable remaining = n
            let mutable divisor = 2

            while remaining > 1 do
                if remaining % divisor = 0 then
                    factors <- divisor :: factors
                    remaining <- remaining / divisor
                else
                    divisor <- divisor + 1

            match factors with
            | [_] -> Prime
            | _ -> Composite(factors)

    let describeNumber n =
        match n with
        | Prime -> printfn "n is prime"
        | Composite(factors) -> printfn "n in composite, with factors %A" factors
        | Neither -> printfn "n is neither prime nor composite"

(* RECURSIVE LOOPS *)

module Example4_functional =
 
    let rec forLoop i =
        match i <= 5 with
        | false -> ()
        | true ->
            printfn "%d" i
            forLoop (i + 1)
    forLoop 1

    let rec whileLoop start =
        match (DateTime.Now - start) < TimeSpan.FromSeconds(2.) with
        | false -> ()
        | true ->
            printfn "Looping..."
            Thread.Sleep(300)
            whileLoop start
    whileLoop DateTime.Now


module Example5_functional =

    let rec whileLoop start (rnd:Random) =
        if not ((DateTime.Now - start) < TimeSpan.FromSeconds(2.)) then
            printfn "Looping..."
            Thread.Sleep(300)
            if rnd.Next() % 10 <> 0 then
                whileLoop start rnd
    whileLoop DateTime.Now (Random())
    

module Example6_functional =

    let factorPositive n = 
        let rec loop remaining divisor factors =
            match (remaining > 1, remaining % divisor) with
            | (false, _) -> factors
            | (_, 0) -> loop divisor (remaining/divisor) (divisor::factors)
            | _ -> loop (divisor + 1) remaining factors
            
        loop n 2 []


module Example3_functional_full = 
    let factorPositive n = 
        let rec loop remaining divisor factors =
            match (remaining > 1, remaining % divisor) with
            | (false, _) -> factors
            | (_, 0) -> loop divisor (remaining/divisor) (divisor::factors)
            | _ -> loop (divisor + 1) remaining factors
            
        loop 2 n []

    let (|Negative|Zero|Positive|) n =
        if n < 0 then Negative
        elif n = 0 then Zero
        else Positive

    let (|Prime|Composite|Neither|) n =
        match n with
        | Zero | 1 | Negative -> Neither
        | Positive ->            
            match factorPositive n with
            | [_] -> Prime
            | factors -> Composite(factors)

    let describeNumber n =
        match n with
        | Prime -> printfn "n is prime"
        | Composite(factors) -> printfn "n in composite, with factors %A" factors
        | Neither -> printfn "n is neither prime nor composite"
        
(* HIGHER-ORDER FUNCTIONS FOR COLLECTION PROCESSING *)

module Example7_functional =
    let myArray = [|"one"; "two"; "three"|]
    let stringLengths =
        myArray |> Array.map (fun elem -> elem.Length)

        
module Example8_functional =
    let myArray = [|"one"; "two"; "three"|]
    let sumOfStringLengths =
        myArray |> Array.fold (fun accum elem -> accum + elem.Length) 0


module Example9_functional = 

    let myArray = [|"one"; "two"; "three"|]
    let filtered =
        myArray |> Array.filter (fun item -> item.Length = 3)

module Example10_functional =

    let myArray = [|"one"; "two"; "three"|]
    myArray
    |> Array.map (fun elem -> (elem, elem.Length))
    |> Array.filter (fun (_, length) -> length = 3)
    |> Array.iter (fun (str, length) ->
            printfn "Length of %A is %d" str length
    )


module OtherFunStuff = 

    let time f = 
        let sw = Stopwatch.StartNew()
        f() |> ignore
        sw.Stop()
        printfn "Elapsed: %A" sw.Elapsed

    let memoize f =
        let cache = Dictionary<_, _>()
        fun x ->
            match cache.TryGetValue(x) with
            | (true, value) -> value
            | _ ->
                let value = f x
                cache.Add(x, value)
                value
