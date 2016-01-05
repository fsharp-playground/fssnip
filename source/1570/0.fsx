let src = [1..2000]

(* Old way *)
let rec selections1 l = 
        match l with 
         | [] -> []
         | (x::xs) -> (x,xs) :: [for (y,ys) in (selections1 xs) -> (y, x::ys)]

#time 

selections1 src |> List.length // 2000
//> Real: 00:00:00.712, CPU: 00:00:00.718, GC gen0: 42, gen1: 10, gen2: 1

(* New Ways *)

let rec selections3 l = 
        match l with 
         | [] -> Seq.empty
         | (x::xs) -> selections3' x xs
and selections3' x xs =
    seq {
        yield x, xs
        for (y,ys) in (selections3 xs) do
            yield (y, x::ys)
    } 
selections3 src |> Seq.length // 2000
// Real: 00:00:00.118, CPU: 00:00:00.125, GC gen0: 30, gen1: 0, gen2: 0
selections3 src |> Seq.take 10


let selections4 l = 
    seq {
        for i, x in l |> Seq.mapi (fun i x -> i, x) do
            yield x, l |> Seq.mapi (fun is e -> is, e)
                       |> Seq.filter (fun (is,e) -> is <> i)
                       |> Seq.map snd
    }

selections4 src |> Seq.length // 2000
//Real: 00:00:00.000, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0
selections4 src |> Seq.take 10

