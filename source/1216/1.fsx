//[snippet:implementation]
open System
type SeqBuilder () =    
  member __.For(m:#seq<_>,f) = Seq.collect f m
  member __.For((m1,m2):#seq<_> * #seq<_>, f) =
     __.For(m1,fun x1 -> Seq.collect(fun x2 -> f(x1,x2)) m2) 
  member __.For((m1,m2,m3):#seq<_> * #seq<_> * #seq<_>, f) = 
    __.For((m1,m2),fun(x1,x2)-> Seq.collect(fun x3 -> f(x1,x2,x3)) m3)
  member __.For((m1,m2,m3,m4): #seq<_> * #seq<_> * #seq<_> * #seq<_>, f) = 
    __.For((m1,m2,m3),fun(x1,x2,x3)-> Seq.collect(fun x4 -> f(x1,x2,x3,x4)) m3)
  
  member __.Yield x = Seq.singleton x
  member __.YieldFrom x = x
  member __.Zero() = Seq.empty
  member __.Delay f = f
  member __.Run f = f ()
  member __.Combine(m1,m2) = Seq.append m1 (m2())
  member __.While(guard, body:_->_ seq) =
    seq { while guard () do yield! body () }

let seq' = SeqBuilder ()
//[/snippet]

//[snippet:usage]
seq' {                       
yield "before nested loop..\r\n"
for i,j,k,l in [0..1],[0..1],[0..1],[0..1] do
  let m = ref 0
  while !m < 2 do
    yield! [ for n in 0..1 -> sprintf "%2d " (i*32 + j*16 + k*8 + l*4 + !m*2 + n) ]
    incr m
  if l = 1 then yield "\r\n"
} |> Seq.iter (printf "%s")
//[/snippet]

//[snippet:result]
//before nested loop..
// 0  1  2  3  4  5  6  7 
// 8  9 10 11 12 13 14 15 
//16 17 18 19 20 21 22 23 
//24 25 26 27 28 29 30 31 
//32 33 34 35 36 37 38 39 
//40 41 42 43 44 45 46 47 
//48 49 50 51 52 53 54 55 
//56 57 58 59 60 61 62 63 
//[/snippet]
