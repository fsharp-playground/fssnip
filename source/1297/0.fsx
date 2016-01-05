let ToMixedNumber(x : float) =
    let wholePart = int x     // whole part of x
    let decimalPt = x % 1.0   // decimal part of x
    let rec cF(Z : float, i : int, Dm : float, Do : float) =
        match Z % 1.0 > 1e-6, i < 1 with
        //  First case terminates after 14 iterations
        | _    , true  -> (wholePart, (int (System.Math.Round(decimalPt * Do)), int Do))
        //  Second case executes next cF (continuing fraction)
        | true , false -> cF(1.0/(Z % 1.0), i - 1 , Do, Do * System.Math.Truncate(1.0/(Z % 1.0))+ Dm )
        //  Final case terminates if the remainder of Z > 10^-6
        | false, _  -> (wholePart, (int (System.Math.Round(decimalPt * Do)), int Do))
    decimalPt
    |> fun x -> cF(x, 14, 0.0, 1.0)    
//  Test using pi as input
let dummyrecA = ToMixedNumber System.Math.PI
//  Recompose pi as a decimal form. 
//  NOTE Actual Value of pi = 3.1415926535897932384626433833M
let myPiA = decimal (fst dummyrecA) + decimal ( fst (snd dummyrecA)) / decimal ( snd (snd dummyrecA))
