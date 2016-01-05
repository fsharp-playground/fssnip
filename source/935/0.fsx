(*[omit:(AgeVector class and builder omitted)]*)

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

(*[/omit]*)

(*[omit:(AgeVector helper functions omitted)]*)
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

(*[/omit]*)

(*[omit:(mortality data omitted)]*)
let pma92vals = [0.00;0.00;0.000235;0.000233;0.000233;0.000231;0.000231;0.000230;0.000229;0.000229;0.000229;
                 0.000229;0.000230;0.000231;0.000233;0.000237;0.000241;
                 0.000247;0.000254;0.000262;0.000274;0.000288;0.000306;0.000328;0.000355;0.000388;0.000428;
                 0.000476;0.000535;0.000605;0.000689;0.000789;0.000908;0.001049;0.001216;0.001413;0.001643;
                 0.001914;0.00223;0.002597;0.003023;0.003516;0.004085;0.004806;0.005642;0.00661;0.007725;
                 0.009006;0.010474;0.012149;0.014054;0.016214;0.018653;0.021399;0.024479;0.027922;0.031756;
                 0.03601;0.040712;0.04589;0.051571;0.05778;0.064539;0.071867;0.079782;0.088295;0.097414;
                 0.107142;0.117477;0.128409;0.139923;0.151999;0.164609;0.177718;0.191285;0.205265;0.219604;
                 0.234247;0.24913;0.264188;0.279353;0.294553;0.309716;0.32477;0.339641;0.35426;0.368556;
                 0.382461;0.395911;0.408847;0.421211;0.432949;0.444014;0.453033;0.461297;0.46878;0.475459;
                 0.481313;0.486326;0.490484;0.493776;0.496194;1.0] 

let pfa92vals = [0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;
                 0.0;0.0;0.0;0.0;0.00014;0.00014;0.00014;
                 0.00014;0.00014;0.00014;0.000141;0.000142;0.000144;0.000146;
                 0.00015;0.000153;0.000159;0.000164;0.000172;0.000182;0.000193;
                 0.000206;0.000222;0.000241;0.000264;0.000291;0.000323;0.000361;
                 0.000405;0.000457;0.000518;0.000589;0.000671;0.000767;0.000878;
                 0.001006;0.001154;0.001324;0.00152;0.001744;0.001999;0.002291;
                 0.002624;0.003001;0.00343;0.003969;0.004585;0.005287;0.006084;
                 0.006989;0.008012;0.009166;0.010465;0.011924;0.013557;0.015381;
                 0.017413;0.019671;0.022174;0.024942;0.027994;0.03135;0.035029;
                 0.039055;0.043445;0.048222;0.053402;0.059006;0.065049;0.071549;
                 0.078518;0.085968;0.093911;0.10235;0.111291;0.120735;0.130678;
                 0.141114;0.152032;0.163418;0.175253;0.187516;0.20018;0.213215;
                 0.226587;0.240259;0.254192;0.26834;0.282658;0.2971;0.311616;
                 0.326156;0.340667;0.3551;0.369403;0.382406;0.39515;0.407594;
                 0.4197;0.431431;0.442752;0.45363;0.464038;0.473947;1.0]
(*[/omit]*)

let discFunc term = fun ps -> (AgeVectorFunctions.discount 0.02 0.03 term) * ps
let pma92 = new AgeVector<double>(18, 120, pma92vals, Extend, Extend)

// Joint life annuity same age assumed
let jointLifeAnnuity = defaultAgeVector {
        let sa = defaultAgeVector.StartAge
        let ea = defaultAgeVector.EndAge
        let lb = defaultAgeVector.LowerBoundBehaviour
        let ub = defaultAgeVector.UpperBoundBehaviour
        let! pma92fn = pma92
        let! pfa92fn = new AgeVector<double>(1, ea, pfa92vals, lb, ub)
        let psFn = fun term -> 
            fun age -> ((AgeVectorFunctions.probSurvival pma92fn term) age)*((AgeVectorFunctions.probSurvival pfa92fn term) age)
        let asl = fun age ->
            [1..(ea - age)]
            |> List.fold  (fun acc a -> 
                    acc + (AgeVectorFunctions.pureEndowment (psFn) discFunc a) age) 0.0
         
        return (asl)
    }

// Joint life with age difference
let ageDiffedSpouse n = defaultAgeVector {
        let sa = defaultAgeVector.StartAge
        let ea = defaultAgeVector.EndAge
        let lb = defaultAgeVector.LowerBoundBehaviour
        let ub = defaultAgeVector.UpperBoundBehaviour
        let! pfa92fn = new AgeVector<double>(1, ea, pfa92vals, lb, ub)
        return (fun age -> pfa92fn (age - n))
    }

let jointLifeAnnuityAgeDiff = defaultAgeVector {
        let sa = defaultAgeVector.StartAge
        let ea = defaultAgeVector.EndAge
        let lb = defaultAgeVector.LowerBoundBehaviour
        let ub = defaultAgeVector.UpperBoundBehaviour
        let! pma92fn = pma92
        let! pfa92fn = ageDiffedSpouse 3
        let psFn = fun term -> 
            fun age -> ((AgeVectorFunctions.probSurvival pma92fn term) age)*
                        ((AgeVectorFunctions.probSurvival pfa92fn term) age)
        let asl = fun age ->
            [1..(ea - age)]
            |> List.fold  (fun acc a -> 
                    acc + (AgeVectorFunctions.pureEndowment (psFn) discFunc a) age) 0.0
         
        return (asl)
    }