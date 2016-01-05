open Price

// Open a window with chart and classifiers
let win = ClassifierWindow()
win.Run("MSFT")
// Stop resets the chart in the window
win.Stop()
// Clear removes all classifiers shown
win.Clear()


// Price is always rising (rarely happens)
win.Add("Always rising", rising)
