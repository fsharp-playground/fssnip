open Price

// Open a window with chart and classifiers
let win = ClassifierWindow()
win.Run("MSFT")
// Stop resets the chart in the window
win.Stop()
// Clear removes all classifiers shown
win.Clear()


// Simple pattern classifiers

// Price is always rising (rarely happens)
win.Add("Always rising", rising)
// Price rising over a linear regression
win.Add("Mostly rising", regression rising)
 

// Classifiers for calculating numeric indicators

// Basic classifiers extract min, max, avg
win.Add("Minimum", minimum)
win.Add("Minimum", maximum)
win.Add("Average", average)

// Calculate difference between min and max
let diff = both minimum maximum |> map (fun (l, h) -> h - l)
win.Add("Difference", diff)


// Detecting interesting patterns 

// Inverse "V" pattern (price goes up, then down)
let upDown = sequenceAnd (regression rising) (regression declining)
win.Add("Up & Down", upDown)

// Classifier checks whether average is less than specified
let averageLessThan lo =
  average |> map (fun v -> v < lo)

// Classifier detects rising price with avg under 26
let risingUnder26 = 
  bothAnd (regression rising) (averageLessThan 26.0)
win.Add("Rising <26", risingUnder26)

// True when difference is greater than specified
let differsBy limit = 
  both minimum maximum |> map (fun (l, h) -> h - l > limit)

// The price is mostly rising and the difference is more than 3
let risingFast = bothAnd (regression rising) (differsBy 3.0)
win.Add("Rising fast", risingFast)


// Computation expression examples

// Price is declining and average is more than 27
let downOver27 = classify { 
  // Calculate average over the range
  let! avg = average
  // Test if the price is mostly declining
  let! down = regression declining
  // Evaluate the condition 
  return down && (avg >= 27.0) }

win.Add("Down >27", downOver27)


// Detecting the "L" patterns & some helpers

// Get the min-max range 
let range = both minimum maximum
// Left side is going down
let leftDown = bothAnd (regression declining) always
win.Add("Left down", leftDown)

// Detect the "L" pattern 
// (Left side goes down & the right side keeps low
// - in range 1/3 from minimum of left side)
let patternL = classify {
  // Get ranges for left & right parts
  let! (lmin, lmax), (rmin, rmax) = sequence range range
  // The left part is declining
  let! decl = leftDown
  
  // The right part keeps in a range
  // (lo +/- of 1/3 difference)
  let offs = (lmax - lmin) / 3.0
  let inRange v = v >= lmin - offs && v <= lmin + offs
  return decl && inRange rmin && inRange rmax } 

win.Add("L pattern", patternL)