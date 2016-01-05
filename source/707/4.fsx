open System.Collections.Generic
open System.Threading

// **************************************************************
// Defines an AgeVector type which "generates" values at each age 
// according to a generator function.
// **************************************************************
type Age = int
type Term = int

let (|ValidAge|InvalidAge|) (age : Age) =
    if age >= 0 && age <= 120 then
        ValidAge (age)
    else
        InvalidAge

type boundaryBehaviour<'T> =
    | Zero of 'T
    | One of 'T
    | Fixed of 'T
    | Extend
    | Fail

type IAgeVector<'T> =
    abstract member StartAge : Age
    abstract member EndAge : Age
    abstract member ValueAtAge : Age -> 'T
    abstract member LowerBoundBehaviour : boundaryBehaviour<'T>
    abstract member UpperBoundBehaviour : boundaryBehaviour<'T>

type AgeVector<'T> (startAge,
                    endAge, 
                    generator,
                    lowerBoundBehaviour,
                    upperBoundBehaviour) =
    
    member private this.boundary boundaryAge = function
        | Zero v -> v
        | One v -> v
        | Fixed(v) -> v
        | Extend -> this.AtAge boundaryAge
        | Fail -> failwith "Requested Age is out of bounds and no substitute value has been declared."
    
    member this.AtAge age = (this :> IAgeVector<'T>).ValueAtAge age
    
    interface IAgeVector<'T> with
        member this.StartAge with get () = startAge
        member this.EndAge with get () = endAge
        member this.ValueAtAge age =
            match age with
            | ValidAge v when v < startAge -> this.boundary startAge lowerBoundBehaviour
            | ValidAge v when v > endAge -> this.boundary endAge upperBoundBehaviour
            | ValidAge v -> generator v
            | _ -> failwith "Invalid age."
        member this.LowerBoundBehaviour with get() = lowerBoundBehaviour
        member this.UpperBoundBehaviour with get() = upperBoundBehaviour
 
    new (startAge, 
         endAge,
         data : seq<'T>,
         lowerBoundBehaviour,
         upperBoundBehaviour) =
        let generator (age : Age) = 
            data 
            |> Seq.nth (age - startAge)
        new AgeVector<'T> (startAge, 
                           endAge, 
                           generator,
                           lowerBoundBehaviour,
                           upperBoundBehaviour)


// ***************************************************************
// Implement builder logic
// ***************************************************************
let bind (av : AgeVector<'T>) (rest : (Age -> 'T) -> AgeVector<'U>) : AgeVector<'U> = rest av.AtAge

type AgeVectorBuilder<'T>(startAge : Age,
                          endAge : Age,
                          lowerBoundBehaviour : boundaryBehaviour<'T>,
                          upperBoundBehaviour : boundaryBehaviour<'T>) =
    member this.StartAge with get () = startAge
    member this.EndAge with get () = endAge
    member this.LowerBoundBehaviour with get () = lowerBoundBehaviour
    member this.UpperBoundBehaviour with get () = upperBoundBehaviour

    member this.Delay(f) = f()
    member this.Return (genFunc : Age -> 'T) = 
        new AgeVector<'T>(startAge, endAge, genFunc, lowerBoundBehaviour, upperBoundBehaviour)
    member this.ReturnFrom(genFunc : Age -> 'T) = genFunc
    member this.Bind (av, rest) = bind av rest
    member this.Let (av, rest) : AgeVector<'T> = rest av        
    
let defaultAgeVector = new AgeVectorBuilder<_>(18, 120, Zero (0.0), Fail)

// ***************************************************************
module AgeVectorFunctions =

    let probSurvival ageVectorFn (term : Term) =
        let psFunc (age : Age) = 
            [age .. (age + term - 1)]
            |> List.fold (fun acc age -> acc * (1.0 - (ageVectorFn age))) 1.0
        psFunc

    let discount pensionIncr intr (term : Term) =
        ((1.0 + pensionIncr) / (1.0 + intr)) ** (double)term

    let pureEndowment (psFunc : Term -> (Age -> double)) (discountToTerm : Term -> (double -> double)) = 
        fun term -> (psFunc term) >> (discountToTerm  term)
        
    let transform f ageVector =
        let genFunc = f << (ageVector :> IAgeVector<_>).ValueAtAge
        let newAgeVector = new AgeVector<_> (
                                ageVector.StartAge,
                                ageVector.EndAge,
                                genFunc,
                                ageVector.LowerBoundBehaviour,
                                ageVector.UpperBoundBehaviour)
        newAgeVector

// *******************************************************************
// Test data - mortality table from 18 - 120. This is simply an extract 
// of the publicly available PMA92 (C=2003) mortality table.
// *******************************************************************
let pma92vals = 
    [0.00;0.00;0.000235;0.000233;0.000233;0.000231;0.000231;0.000230;
     0.000229;0.000229;0.000229;0.000229;0.000230;0.000231;0.000233;
     0.000237;0.000241;0.000247;0.000254;0.000262;0.000274;0.000288;
     0.000306;0.000328;0.000355;0.000388;0.000428;0.000476;0.000535;
     0.000605;0.000689;0.000789;0.000908;0.001049;0.001216;0.001413;
     0.001643;0.001914;0.00223;0.002597;0.003023;0.003516;0.004085;
     0.004806;0.005642;0.00661;0.007725;0.009006;0.010474;0.012149;
     0.014054;0.016214;0.018653;0.021399;0.024479;0.027922;0.031756;
     0.03601;0.040712;0.04589;0.051571;0.05778;0.064539;0.071867;
     0.079782;0.088295;0.097414;0.107142;0.117477;0.128409;0.139923;
     0.151999;0.164609;0.177718;0.191285;0.205265;0.219604;0.234247;
     0.24913;0.264188;0.279353;0.294553;0.309716;0.32477;0.339641;
     0.35426;0.368556;0.382461;0.395911;0.408847;0.421211;0.432949;
     0.444014;0.453033;0.461297;0.46878;0.475459;0.481313;0.486326;
     0.490484;0.493776;0.496194;1.0]

//// some Tests
let discFunc term = fun ps -> (AgeVectorFunctions.discount 0.02 0.03 term) * ps
let pma92 = new AgeVector<double>(18, 120, pma92vals, Extend, Extend)

let simpleScaling = defaultAgeVector {
        let halveIt = fun dblVal -> dblVal*0.5
        let! pma92fn = pma92
        return (pma92fn >> halveIt)}

let simpleShift n = defaultAgeVector {
        let! pma92fn = pma92
        return (fun age -> pma92fn (age - n))}

let singleLifeAnnuity = defaultAgeVector {
        let ea = defaultAgeVector.EndAge
        let! pma92fn = pma92
        let psFn = fun term -> AgeVectorFunctions.probSurvival pma92fn term
        let asl = fun age ->
            [1..(ea - age)]
            |> List.fold  (fun acc a -> 
                    let pe = (AgeVectorFunctions.pureEndowment (psFn) discFunc a) age
                    acc + (AgeVectorFunctions.pureEndowment (psFn) discFunc a) age) 0.0
         
        return (asl)
    }

