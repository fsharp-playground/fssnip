// [snippet: Primitive Pythagorean triples generator]
#r "FSharp.PowerPack.dll"
open Microsoft.FSharp.Math

// Pythagorean triples : http://en.wikipedia.org/wiki/Pythagorean_triple
// Algorithm: http://mathworld.wolfram.com/PythagoreanTriple.html Equations(2 .. 9)

// Primitive Pythagorean triples generator
let primitives take =
    let org = rowvec [ 3.; 4.; 5. ]
    
    let U = matrix [[  1.; 2.; 2. ];
                    [ -2.;-1.;-2. ];
                    [  2.; 2.; 3. ];]

    let A = matrix [[  1.; 2.; 2. ];
                    [  2.; 1.; 2. ];
                    [  2.; 2.; 3. ];]

    let D = matrix [[ -1.;-2.;-2. ];
                    [  2.; 1.; 2. ];
                    [  2.; 2.; 3. ];]

    let triplets (p:RowVector<float>) = (p*U,p*A,p*D)
    
    let rec primitives' next cont acc = 
        if take next then
            let u,a,d = triplets next
            next::acc |> primitives' u (primitives' a (primitives' d cont))
        else
            cont acc
    
    primitives' org id []
// [/snippet]    

// [snippet: Example: Project Euler 75]
// http://projecteuler.net/index.php?section=problems&id=75
let limit = 1500000.

let perimeter (t :RowVector<_>) = t.[0] + t.[1] + t.[2]

let perimeterUnder1500000 t = perimeter t <= limit

let solution = primitives perimeterUnder1500000
               |> Seq.map perimeter
               // include the perimeter of the primitive and all the multiples
               |> Seq.map(fun p -> seq { p .. p .. limit })
               |> Seq.concat
               |> Seq.countBy id
               |> Seq.filter(snd >> (=) 1)
               |> Seq.length
solution |> printfn "solution: %d"
// [/snippet]
