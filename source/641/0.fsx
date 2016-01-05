let (-~) a1 a2 = 
    let sq x = x * x
    Array.fold2 (fun acc n1 n2 -> acc + sq (n1 - n2) ) 0. a1 a2

// Examples:

let ones = [|1.; 1.; 1.|]
let twos = [|2.; 2.; 2.|]
let oneTwoThree = [|1.; 2.; 3.|]
let oneishTwoishThreeish =  [|1.1; 1.9; 2.99999|]
let big = [|100.; 101.; 100.|]

// Simple comparisons:
let onesVersusOnes = ones -~ ones // 0.0
let onesVersusTwos = ones -~ twos // 1.0
let twosVersusOnes = twos -~ ones // 0.0
let sameish = oneTwoThree -~ oneishTwoishThreeish // 1e-10
let veryDifferent = ones -~ big // 9801.0

// Order a list of arrays based on their similarity to a 'seed' array:
let ordered =
    let seed = twos
    let list = [oneishTwoishThreeish; big; ones; oneTwoThree]
    list
    |> List.sortBy (fun arr -> arr -~ seed)

// [[|1.1; 1.9; 2.99999|]; [|1.0; 2.0; 3.0|]; [|1.0; 1.0; 1.0|]; [|100.0; 101.0; 100.0|]]

