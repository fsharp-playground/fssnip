//type Variable = | WA | NT | Q | NSW | V | SA | T
//type Domain = | Red | Green | Blue
//type BinaryConstraint = | NotEqualTo of left: Variable * right: Variable | NotValue of variable: Variable * value: Domain
//
//let X = [ WA; NT; Q; NSW; V; SA; T ] //is a set of variables 
//let D = [ Red; Green; Blue ] //is a set of the respective domains of Domains, and 
//let C = [ NotValue(WA, Red); NotEqualTo(WA, NT); NotEqualTo(WA, SA); NotEqualTo(NT, SA); NotEqualTo(NT, Q); NotEqualTo(SA, Q); NotEqualTo(SA, NSW); NotEqualTo(SA, V); NotEqualTo(Q, NSW); NotEqualTo(NSW, V); NotEqualTo(T, V) ] //is a set of constraints
//
//type CSP = {X: Variable list;  D: Domain list; C: BinaryConstraint list}
//let csp = {X = X; D = D; C = C}

/////////////////////////////////////////////

type Variable = | WA | NT | Q | NSW | V | SA | T
type Domain = | Red | Green | Blue
type BinaryConstraint = | NotEqualTo of left: Variable * right: Variable | NotValue of variable: Variable * value: Domain

let X = [ WA; NT; Q; NSW; V; SA; T ] //is a set of variables 
let D = [ Red; Green; Blue ] //is a set of the respective domains of Domains, and 
let C = [ NotEqualTo(WA, NT); NotEqualTo(WA, SA); NotEqualTo(NT, SA); NotEqualTo(NT, Q); NotEqualTo(SA, Q); NotEqualTo(SA, NSW); NotEqualTo(SA, V); NotEqualTo(Q, NSW); NotEqualTo(NSW, V); NotEqualTo(T, V) ] //is a set of constraints

type CSP = {X: Variable list;  D: Domain list; C: BinaryConstraint list}
let csp = {X = X; D = D; C = C}

/////////////////////////////////////////////

//type Variable = | X1 | X2 | X3
//type Domain = | Red | Green | Blue
//type BinaryConstraint = | NotEqualTo of left: Variable * right: Variable | NotValue of variable: Variable * value: Domain
//
//let X = [ X1; X2; X3 ] //is a set of variables 
//let D = [ Red; Green; Blue ] //is a set of the respective domains of Domains, and 
//let C = [ NotEqualTo(X1, X2); NotEqualTo(X1, X3); NotEqualTo(X2, X3) ] //is a set of constraints
//
//type CSP = {X: Variable list;  D: Domain list; C: BinaryConstraint list}
//let csp = {X = X; D = D; C = C}

/////////////////////////////////////////////

let combine (xs:Variable list, ds: Domain list) =
    let rec combineRec (ys:Variable list, acc: (Variable * Domain) list) =
        match ys with
        | y :: ys -> seq { for d in ds do
                           for c in combineRec(ys, List.append acc [(y, d)]) do yield c }
        | [] -> Seq.ofList [acc]
    combineRec (xs, [])

let evaluate (xs:(Variable * Domain) list seq, cs: BinaryConstraint list) =
    let notEqualTo (left, right) = left <> right
    seq{ for x in xs ->
          [for c in cs ->
            match c with
            | NotValue(variable, value) ->
                let v = x |> List.find (fun (v, _) -> v = variable) |> snd
                notEqualTo(v, value)
            | NotEqualTo(left, right) ->
                let a = x |> List.find (fun (v, _) -> v = left)  |> snd
                let b = x |> List.find (fun (v, _) -> v = right) |> snd 
                notEqualTo(a, b)] }

let depthFirst (xs: Variable list, ds: Domain list, cs: BinaryConstraint list) =
    let all xs = xs |> Seq.forall (fun x -> x)
    let rec depthFirstRec (ys:((Variable * Domain) list * bool list) seq) =
        if (Seq.isEmpty ys) then []
        else
            let y = ys |> Seq.head            
            if all(snd y) then fst y 
            else depthFirstRec(ys |> Seq.skip 1)    
    let xds = combine(xs, ds)
    depthFirstRec (Seq.zip xds (evaluate(xds, cs)))

let backtrackingSearch (csp : CSP) = depthFirst(csp.X, csp.D, csp.C)

//main
printfn "%A" (backtrackingSearch csp)
