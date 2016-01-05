namespace Algs4.Queue

open System.Collections
open System.Collections.Generic

exception Empty

type IDeque<'a> = 
   abstract IsEmpty   : bool
   abstract pushFront : 'a->IDeque<'a>
   abstract pushBack  : 'a->IDeque<'a>
   abstract popFront  :  unit-> 'a*IDeque<'a>
   abstract popBack   :  unit-> 'a*IDeque<'a>

type BatchDeque<'a> (front,rear) = 
   interface IDeque<'a> with
      member x.IsEmpty = 
         match front, rear with 
         | [], []  -> true
         | _       -> false

      member x.pushFront v = BatchDeque(v::front,    rear) :> _
      member x.pushBack  v = BatchDeque(   front, v::rear) :> _
      member x.popFront () = 
         match front, rear with
         | x::xs,   _ -> x, BatchDeque(xs, rear) :> _
         | []   ,  [] -> raise Empty
         | []   ,   _ -> //splits rear in two, favoring rearbot for odd length
                         let _, reartop, rearbot = List.fold(fun (i, reartop, rearbot) e -> 
                                                              if i < rear.Length / 2 
                                                              then (i+1, e::reartop, rearbot)
                                                              else (i+1, reartop, e::rearbot)) (0,[],[]) rear
                         printfn "reartop, rearbot  %A %A" reartop rearbot
                         let rear', (x::front')  = reartop |> List.rev , rearbot //|> List.rev
                         x, BatchDeque(front', rear') :> _
      member x.popBack () = 
         match front, rear with 
         | _ , x::xs -> x, BatchDeque(front, xs) :> _
         | []   ,  [] -> raise Empty
         | _   ,   [] -> //splits front in two, favoring frontbot for odd length
                         let _, fronttop, frontbot = List.fold(fun (i, reartop, rearbot) e -> 
                                                               if i < rear.Length /2
                                                               then (i+1, e::reartop, rearbot)
                                                               else (i+1, reartop, e::rearbot)) (0,[],[]) front
                         let front', (x::rear') = fronttop |> List.rev , frontbot
                         x, BatchDeque(front', rear') :> _         