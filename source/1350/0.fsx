type Variable = char * int // symbol * value
type Term = int * Variable list // coefficient * variables
type Equation = Term list * int * int // terms * constant-term * result

let genEquationSet numVars =
    let rng = new System.Random()
    let vars : Variable[] =
        "acdefghijkmnpqrtuvwxy".ToCharArray() // avoid letters that could be confused with numbers
        |> Array.sortBy(fun _ -> rng.Next())
        |> Seq.take numVars
        |> Seq.map (fun ch -> ch, rng.Next(-25, 26)) // range of values that variables may take
        |> Seq.toArray
    let rec triangular freeVars accum =
        match freeVars with
        | 0 -> accum
        | _ ->
            let constTerm =
                if rng.NextDouble() > 0.3 then rng.Next(-99,100) // constant term range of values
                else 0
            let simpleTerms, simpleResult =
                let coeffs =
                    Seq.initInfinite (fun _ -> rng.Next(-15,16)) // range of values for term coefficients
                    |> Seq.filter ((<>) 0)
                    |> Seq.take freeVars
                    |> Seq.toArray
                let terms : Term list =
                    vars
                    |> Seq.take freeVars
                    |> Seq.mapi (fun i (ch,value) -> coeffs.[i], [ch,value])
                    |> Seq.toList
                let result =
                    terms |> Seq.fold (fun state (cf,[_,v]) -> state + cf*v) 0
                terms, result
            let e : Equation = simpleTerms, constTerm, simpleResult+constTerm
            triangular (freeVars-1) (e::accum)
            |> List.rev
    let stringifyEqn (e : Equation) =
        let terms, constterm, result = e
        let sb = System.Text.StringBuilder()
        for (cf,vars) in terms do
            if cf >= 0 then sb.Append '+' |> ignore
            sb.Append cf |> ignore
            for (sym,_) in vars do
                sb.Append sym |> ignore
        if constterm > 0 then sb.AppendFormat("+{0}",constterm) |> ignore
        elif constterm < 0 then sb.Append constterm |> ignore
        sb.AppendFormat("={0}",result) |> ignore
        sb.ToString().TrimStart('+')
    triangular vars.Length []
    |> List.map stringifyEqn

// usage:
// > genEquationSet 3;;
// val it : string list = ["14u-12i+2q+59=149"; "11u-1i=124"; "10u=120"]
// > genEquationSet 2;;
// val it : string list = ["-3j-82=-97"; "-12j+7y-74=-295"]
// > genEquationSet 4;;
// val it : string list = ["10h+81=161"; "15h-15w+7=382"; "10h-14w+6g-28=356"; "-6h+9w+4g-13f+97=18"]