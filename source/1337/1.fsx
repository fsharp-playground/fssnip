#r @"..\packages\numl.0.7.5\lib\net40\numl.dll"

open numl
open numl.Model
open numl.Supervised.DecisionTree

type Outlook =
    | Sunny = 0
    | Overcast = 1
    | Rainy = 2

type Temperature =
    | Low = 0
    | High = 1

type Tennis =
    {
        [<Feature>] Outlook : Outlook
        [<Feature>] Temperature : Temperature
        [<Feature>] Windy : bool
        [<Label>] Play : bool
    }

    static member New outlook temperature windy play =
        box {
            Outlook = outlook
            Temperature = temperature
            Windy = windy
            Play = play
        }

let data =
    [
        Tennis.New Outlook.Sunny Temperature.Low true true
        Tennis.New Outlook.Sunny Temperature.High true false
        Tennis.New Outlook.Sunny Temperature.High false false
        Tennis.New Outlook.Overcast Temperature.Low true true
        Tennis.New Outlook.Overcast Temperature.High false true
        Tennis.New Outlook.Overcast Temperature.Low false true
        Tennis.New Outlook.Rainy Temperature.Low true false
        Tennis.New Outlook.Rainy Temperature.Low false true
    ]

let descriptor = Descriptor.Create<Tennis>()
let generator = DecisionTreeGenerator descriptor
generator.SetHint false
let model = Learner.Learn(data, 0.8, 1000, generator)

printfn "%A" model

//Learning Model:
//  Generator numl.Supervised.DecisionTree.DecisionTreeGenerator
//  Model:
//	[Outlook, 0,3380]
//	 |- Sunny
//	 |	[Temperature, 1,0000]
//	 |	 |- Low
//	 |	 |	 +(True, 1)
//	 |	 |- High
//	 |	 |	 +(False, -1)
//	 |- Overcast
//	 |	 +(True, 1)
//	 |- Rainy
//	 |	[Windy, 1,0000]
//	 |	 |- False
//	 |	 |	 +(True, 1)
//	 |	 |- True
//	 |	 |	 +(False, -1)
//
//  Accuracy: 100,00 %