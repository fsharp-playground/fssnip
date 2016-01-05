#if INTERACTIVE
#r "../packages/FSharp.Data.2.0.5/lib/net40/FSharp.Data.dll"
#else
module QuizGame
#endif

open System
open FSharp.Data

let bank = WorldBankData.GetDataContext()
let countries = bank.Regions.World.Countries
                |> Seq.filter(fun i -> i.CapitalCity <> "") 
                |> Seq.map(fun i -> i.Name, i.CapitalCity)
let corrects = countries |> Seq.toArray
let rnd = Random()
let questionItem() = corrects.[rnd.Next(corrects.Length)]

let generateQuiz() =
    let correct, capitalCity = questionItem()
    capitalCity, [correct; fst(questionItem()); fst(questionItem())] |> List.sort

let checkAnswer country capitalCity =
    corrects |> Array.exists(fun (cou, cap) -> cou = country && cap = capitalCity)
