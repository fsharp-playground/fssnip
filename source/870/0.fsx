let sa = [ 1 .. 10 ]

let rec fold f s xs = 
   match xs with 
   | [] -> s
   | x::xs  ->  fold  f (f s x) xs
fold (+) 0 sa

let rec foldps f s xs k = 
   match xs with 
   | [] -> k s 
   | x::xs  -> foldps f s xs (fun s  -> f x s k)  

let pluscps = (fun x s cont -> cont(s + x))
foldps pluscps 0 sa id

let rec foldpsn fs s xs k = 
   match xs with 
   | [] -> k s 
   | x::xs  -> match fs with
               | f::[] -> foldps  f  s xs (fun s  -> f x s k)
               | f::fs -> foldpsn fs s xs (fun s  -> f x s k)
               | _     -> failwith "not good"


let printcps = (fun x s cont -> printfn "%A" x;cont(s))
foldpsn [printcps;pluscps] 0 sa id
