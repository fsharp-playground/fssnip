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

let diff = both minimum maximum |> map (fun (l, h) -> h - l)
win.Add("Difference", diff)

let averageInRange (lo, hi) =
  average |> map (fun v -> v > lo && v < hi)
let risingAround20 = 
  bothAnd rising (averageInRange (19.0, 21.0))
win.Add("Rising ~20", risingAround20)

win.Clear()