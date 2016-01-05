let win = ClassifierWindow()
 
let downUp = (Price.sequenceAnd (Price.regression Price.declining) (Price.regression Price.rising)) 
let upDown = (Price.sequenceAnd (Price.regression Price.rising) (Price.regression Price.declining)) 

win.Add("Rising", Price.regression Price.rising)
win.Add("Declining", Price.regression Price.declining)
win.Add("Down & Up", downUp)
win.Add("Up & Down", upDown)
win.Add("W pattern", Price.sequenceAnd downUp downUp)
win.Add("Minimum", Price.minimum)
win.Add("Average", Price.average)

win.Clear()
win.Run("AAPL")
win.Run("MSFT")
win.Stop()