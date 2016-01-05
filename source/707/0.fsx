// A type specifically for Annuity calculation values
// and some supporting functions.
// Not particularlry necessary but I'm using for completeness
type AnnuityVector = AnnuityVector of Map<int, double>
let toMap = function
    | AnnuityVector (a) -> a
let sum = function
    | AnnuityVector (a) -> 
        a 
        |> Map.toSeq
        |> Seq.fold (fun acc (k,v) -> acc + v) 0.0
let valueAtAge age = function
    | AnnuityVector (a) -> a.[age]


// Discount: Small helper function to calculate the discount rate over a number of years (term)
// due to the net of pension increaes and interest rate.
let discount pensionIncr intr (term : int) =
    ((1.0 + pensionIncr) / (1.0 + intr)) ** (float)term


// Probability of survival: given a number of years and a mortality table tells you how likely the life is to survive
// to the end of that period
// term: a number of years
// mortaltyTable: In this example the mortality table is a table of Qx values (prob of death) 
let probSurvival term mortalityTable =
    // for zero term just intialise a Map
    if term = 0 then
        let survivalCertain = Seq.init 120 (fun idx -> idx, 1.0) 
        survivalCertain |> Map.ofSeq |> AnnuityVector
    else
        // take a window of term values and multiply (1-value) to 
        // get the probability of survival to age+term
        let survival = 
            mortalityTable
            |> Map.toSeq
            |> Seq.windowed term    
            |> Seq.map (fun arr ->  
                let srvProd = 
                    arr 
                    |> Array.fold (fun acc el ->
                                let age, survProd = (fst acc), ((snd acc) * (1.0 - (snd el)))
                                age, survProd) ((fst arr.[0]), 1.0)
                srvProd)
            |> Map.ofSeq |> AnnuityVector
        survival

// Pure endowment: A standard calculation of a pure endowment over a number of years (term)
// intr: a fixed interest rate (a more interesting version would take a time-varying sequence of interest rates)
// penIncr: an assumed fixed increment in pension over time (e.g. index-linkage, again we could assign a time-varying sequence)
// mortalityTable: In this example the mortality table is a table of Qx values (prob of death) 
// term: the term (in years) over which to calculate the endowment's value.
let pureEndowment intr pensionIncr mortalityTable  term =
    let disc = discount pensionIncr intr term
    mortalityTable
    |> probSurvival term
    |> toMap
    |> Map.map (fun k v -> v*disc)
    |> AnnuityVector

// Single Life Annuity calculation: This standard actuarial function accepts;
// intr: a fixed interest rate (a more interesting version would take a time-varying sequence of interest rates)
// penIncr: an assumed fixed increment in pension over time (e.g. index-linkage, again we could assign a time-varying sequence)
// mortalityTable: In this example the mortality table is a table of Qx values (prob of death) 
// age: Age of the life
let singleLifeAnnuity intr penIncr mortalityTable age = 
    seq {
            for term in 1 .. (120 - age) do
                let peAtAge = (pureEndowment intr penIncr mortalityTable term) |> valueAtAge age
                yield peAtAge
        }
    |> Seq.sum
    |> (+) 0.5  // An adjustment term to move the calculation to mid-year, whether this is required is 
                // a property of the particular rules your scheme applies.

// set up mortality table of Qx values
let mort = 
    [(1, 0.0);(2, 0.0);(3, 0.0);(4, 0.0);(5, 0.0);(6, 0.0);(7, 0.0);(8, 0.0);(9, 0.0);(10, 0.0);(11, 0.0);(12, 0.0);
    (13, 0.0);(14, 0.0);(15, 0.0);(16, 0.0);(17, 0.0);(18, 0.0);(19, 0.0);(20, 0.000109);(21, 0.000108);(22, 0.000108);
    (23, 0.000107);(24, 0.000107);(25, 0.000107);(26, 0.000107);(27, 0.000106);(28, 0.000106);(29, 0.000107);(30, 0.000107);
    (31, 0.000107);(32, 0.000108);(33, 0.00011);(34, 0.000112);(35, 0.000115);(36, 0.000118);(37, 0.000122);(38, 0.000127);
    (39, 0.000134);(40, 0.000142);(41, 0.000152);(42, 0.000165);(43, 0.00018);(44, 0.000199);(45, 0.000221);(46, 0.000248);
    (47, 0.000281);(48, 0.00032);(49, 0.000366);(50, 0.000422);(51, 0.000487);(52, 0.000565);(53, 0.000656);(54, 0.000763);
    (55, 0.000889);(56, 0.001036);(57, 0.001206);(58, 0.001404);(59, 0.001633);(60, 0.001897);(61, 0.002323);(62, 0.00283);
    (63, 0.003433);(64, 0.004145);(65, 0.004983);(66, 0.005965);(67, 0.007112);(68, 0.008443);(69, 0.009983);(70, 0.011757);
    (71, 0.013792);(72, 0.016116);(73, 0.01876);(74, 0.021753);(75, 0.02513);(76, 0.028921);(77, 0.033162);(78, 0.037883);
    (79, 0.043118);(80, 0.048897);(81, 0.05525);(82, 0.062203);(83, 0.069779);(84, 0.077998);(85, 0.086875);(86, 0.096421);
    (87, 0.106639);(88, 0.117529);(89, 0.129081);(90, 0.141281);(91, 0.154106);(92, 0.167526);(93, 0.181505);(94, 0.195998);
    (95, 0.210957);(96, 0.226322);(97, 0.242034);(98, 0.258024);(99, 0.274221);(100, 0.290551);(101, 0.306936);(102, 0.323298);
    (103, 0.339556);(104, 0.355633);(105, 0.371449);(106, 0.386926);(107, 0.401992);(108, 0.416574);(109, 0.430603);
    (110, 0.444014);(111, 0.453033);(112, 0.461297);(113, 0.46878);(114, 0.475459);(115, 0.481313);(116, 0.486326);
    (117, 0.490484);(118, 0.493776);(119, 0.496194);(120, 1.0)]
    |> Map.ofList

// finally let's test the above for a number of ages:
printf "SingleLifeAnnuity data @ 18: %A\n" (singleLifeAnnuity 0.03 0.0 mort 18)
printf "SingleLifeAnnuity data @ 18: %A\n" (singleLifeAnnuity 0.03 0.0 mort 20)
printf "SingleLifeAnnuity data @ 18: %A\n" (singleLifeAnnuity 0.03 0.0 mort 33)
printf "SingleLifeAnnuity data @ 18: %A\n" (singleLifeAnnuity 0.03 0.0 mort 45)
printf "SingleLifeAnnuity data @ 20: %A\n" (singleLifeAnnuity 0.03 0.0 mort 56)
printf "SingleLifeAnnuity data @ 65: %A\n" (singleLifeAnnuity 0.03 0.0 mort 65)

