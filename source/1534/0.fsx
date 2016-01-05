let f x y = if x = y then 1.0 else 0.0

let cmp = [|"one"; "two"; "three"; "four"|]
let cmps = 
    [| for i = 0 to cmp.Length - 1 do 
           for j = i to cmp.Length - 1 do 
               // Longer array is second
               yield cmp.[j..], cmp.[i..] 
    |] 

#time
for i = 0 to 1000000 do
    for c1,c2 in cmps do 
        bestRot f c1 c2 |> ignore
//Real: 00:00:41.800, CPU: 00:00:41.812, 
// GC gen0: 10175, gen1: 6, gen2: 1
#time 

#time
for i = 0 to 1000000 do
    for c1,c2 in cmps do 
        bestRotGos f c1 c2 |> ignore
//Real: 00:00:06.378, CPU: 00:00:06.375, 
// GC gen0: 689, gen1: 1, gen2: 0
#time 

