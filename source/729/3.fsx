open Price

let win = ClassifierWindow()
win.Run("MSFT")
win.Stop()

win.Add("Always rising", rising)
win.Add("Mostly rising", regression rising)
 
let upDown = sequenceAnd (regression rising) (regression declining)
win.Add("Up & Down", upDown)

win.Add("Minimum", minimum)
win.Add("Minimum", maximum)
win.Add("Average", average)
win.Clear()

let diff = both minimum maximum |> map (fun (l, h) -> h - l)
win.Add("Difference", diff)

let averageLessThan lo =
  average |> map (fun v -> v < lo)
let risingUnder26 = 
  bothAnd (regression rising) (averageLessThan 26.0)
win.Add("Rising <26", risingUnder26)