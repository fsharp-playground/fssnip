//simple factorial
let rec factorial n = 
    if n = 0I then 1I else n * factorial (n-1I)
factorial 10I


//tied and untied factorial - equivalent to simple recursion
let rec factorialu factorial' n = 
    if n = 0I then 1I else n * factorial' (n-1I)

let rec y f x = f  (y f ) x
y factorialu 10I 

//dynamic programming
open System.Collections.Generic
let memoize (d:Dictionary<_,_>) f n = 
   if d.ContainsKey n then d.[n] 
   else System.Threading.Thread.Sleep(10) // to slow down
        d.Add(n, f n)
        d.[n]

let factorialdp  = 
    let d =  Dictionary<bigint, bigint>()// Seq.empty
    fun n ->  
        y (factorialu >> fun f n -> memoize d f n ) n
    
#time
factorialdp 15I  //242 ms

factorialdp 15I  //0 ms

