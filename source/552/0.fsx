

/// one liner tennis kata
let play x y = Seq.map([y,x,"A ";x,y,"B "].get_Item>>fun(w,l,p)->p+match !w with|3->(if !l=3 then l:=2;"advantage"else"win!")|2 when !l=3->w:=3;"deuce"|p->incr w;["15";"30";"40"].[p])


/// A wins a point
let POINT_A = 0
/// B wins a point
let POINT_B = 1
/// helper function to make game points
let cycle points = seq {while true do yield! points}


// A wins every point
let game1 = cycle [POINT_A]
// A and B swap points identically
let game2 = cycle [POINT_A; POINT_B]
// A and B trade points but A wins more points than B
let game3 = cycle [POINT_A; POINT_B; POINT_A]

/// print out the game result
let print = String.concat"!, ">>printfn"%s"

game1 |> play (ref 0) (ref 0) |> Seq.take 10 |> print
// result: A 15!, A 30!, A 40!, A win!
game2 |> play (ref 0) (ref 0) |> Seq.take 10 |> print
// result: A 15!, B 15!, A 30!, B 30!, A 40!, B deuce!, A advantage!, B deuce!, A advantage!, B deuce!
game3 |> play (ref 0) (ref 0) |> Seq.take 10 |> print
// result: A 15!, B 15!, A 30!, A 40!, B 30!, A win!


// simulate 1000 games and show how many times player A and player B wins.
let randomGame = 
    let rnd = new System.Random() 
    seq { while true do yield if rnd.NextDouble() < 0.5 then POINT_A else POINT_B }
seq {
for i in 1 .. 1000 do 
  yield randomGame |> play(ref 0)(ref 0) |> Seq.find(Seq.forall((<>)'!'))
}
|> Seq.groupBy(Seq.forall((<>)'A'))
|> Seq.iter(fun (k,v) -> printfn "%s wins %d times." (if k then "A" else "B") (Seq.length v))
// result:
// A wins 498 times.
// B wins 502 times.