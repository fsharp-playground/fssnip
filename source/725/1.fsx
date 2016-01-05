open Price

let win = ClassifierWindow()
 
let downUp = sequenceAnd (regression declining) (regression rising) 
let upDown = sequenceAnd (regression rising) (regression declining)

win.Add("Rising", regression rising)
win.Add("Declining", regression declining)
win.Add("Down & Up", downUp)
win.Add("Up & Down", upDown)
win.Add("W pattern", sequenceAnd downUp downUp)
win.Add("Minimum", minimum)
win.Add("Minimum", maximum)
win.Add("Average", average)

let differsBy limit = both minimum maximum |> map (fun (l, h) -> h - l > limit)
win.Add("Rising fast", bothAnd (regression rising) (differsBy 3.0))
win.Add("Declining fast", bothAnd (regression declining) (differsBy 3.0))

win.Add("Difference", both minimum maximum |> map (fun (l, h) -> h - l))

win.Clear()
win.Run("AAPL")
win.Run("MSFT")
win.Stop()