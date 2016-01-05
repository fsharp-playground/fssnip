let n_choose_k n k = let rec choose lo  =
                         function
                         |0 -> [[]]
                         |i -> [for j in lo .. (Array.length n)-1 do
                                     for ks in choose (j+1) (i-1) do
                                       yield n.[j] :: ks ]
                     in choose 0  k                           
                         

(* example
n_choose_k [|'a' .. 'f'|] 4 ;;
Real: 00:00:00.000, CPU: 00:00:00.015, GC gen0: 0, gen1: 0, gen2: 0
val it : char list list =
  [['a'; 'b'; 'c'; 'd']; ['a'; 'b'; 'c'; 'e']; ['a'; 'b'; 'c'; 'f'];
   ['a'; 'b'; 'd'; 'e']; ['a'; 'b'; 'd'; 'f']; ['a'; 'b'; 'e'; 'f'];
   ['a'; 'c'; 'd'; 'e']; ['a'; 'c'; 'd'; 'f']; ['a'; 'c'; 'e'; 'f'];
   ['a'; 'd'; 'e'; 'f']; ['b'; 'c'; 'd'; 'e']; ['b'; 'c'; 'd'; 'f'];
   ['b'; 'c'; 'e'; 'f']; ['b'; 'd'; 'e'; 'f']; ['c'; 'd'; 'e'; 'f']]
> 
*)