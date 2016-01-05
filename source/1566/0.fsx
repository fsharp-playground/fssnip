let langs1 = "en,en-GB,de...other langs".Split([|','|])
let langs2 = "en,fr,pt,pt-BR...other langs".Split([|','|])
let langs = Seq.distinct <| seq {yield! langs1; yield! langs2}
let strLang = System.String.Join(",", langs)
printfn "%A" strLang