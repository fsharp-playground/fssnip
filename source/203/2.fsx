module List=
    (* Execution: fast, Output: least intuitive *)
    let split s = 
        let rec loop = function
        | (_,[],a,b) -> (a,b)
        | (true,h::t,a,b) -> loop(false,t,h::a,b)
        | (false,h::t,a,b)-> loop (true,t,a,h::b)
        in loop(true,s,[],[])
    (* Execution: slowest, Output: most intuitive *)
    let split1 s =
        let len = List.length s in
        s |> List.mapi (fun i x -> (i<len/2,x))
          |> List.partition fst
          |> (fun (x,y) -> ((x |> List.map snd),(y |> List.map snd)))
    (* Execution: average, Output: fairly intuitive *)
    let split2 s =
         let len = List.length s in
         s |> List.fold (fun (n,(a,b)) x -> 
            (n+1,if n <(len/2) then (x::a,b) else (a,x::b)))
                  (0,([],[]))
           |> snd
    (* Execution: quite fast, Output: short and elegant *)
    let split3 s = List.fold (fun (xs, ys) e -> (e::ys, xs)) ([], []) s
    (* Execution: fastest, Output: fairly intuitive, but inner function needed *)
    let split4 s =
        let rec loop l (xs, ys) =
            match l with
            | x::y::rest -> loop rest (x::xs, y::ys)
            | [x] -> (x::xs, ys)
            | _ -> (xs, ys)
        loop s ([], [])
            

(*[omit:Timing function]*)
let timeit func param =
    let sw = new System.Diagnostics.Stopwatch()
    sw.Start()
    let _, _ = func param
    sw.Stop()
    sw.ElapsedMilliseconds
(*[/omit]*)

let l = [1..10000000]

printfn "%d" (timeit List.split l)
printfn "%d" (timeit List.split1 l)
printfn "%d" (timeit List.split2 l)
printfn "%d" (timeit List.split3 l)
printfn "%d" (timeit List.split4 l)

(*
Results:
List.split:  1234ms
List.split1: 5242ms
List.split2: 3235ms
List.split3: 2754ms
List.split4: 1068ms
*)

(*
> List.split [1..10];;
val it : int list * int list = ([9; 7; 5; 3; 1], [10; 8; 6; 4; 2])
> List.split1 [1..10];;
val it : int list * int list = ([1; 2; 3; 4; 5], [6; 7; 8; 9; 10])
> List.split2 [1..10];;
val it : int list * int list = ([5; 4; 3; 2; 1], [10; 9; 8; 7; 6])
> List.split3 [1..10];;
val it : int list * int list = ([10; 8; 6; 4; 2], [9; 7; 5; 3; 1])
> List.split4 [1..10];;
val it : int list * int list = ([9; 7; 5; 3; 1], [10; 8; 6; 4; 2])
> 
*)