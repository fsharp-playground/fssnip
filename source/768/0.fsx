// --------------------------------------------------------
// Downlaod Facebook stock prices & draw line chart

let fb = Yahoo.GetPrices("FB")
Plot.Line(fb)
Plot.Clear()

// TASK #1: Download 'AAPL' data and draw the chart
// (...)

// --------------------------------------------------------
// Do some calculations with Facebook stock prices

let count = Seq.length fb
let sum = Seq.sum fb
let avg = sum / float count

// Declare a function that calculates the average
let average data = 
  let count = Seq.length data
  let sum = Seq.sum data
  sum / float count

average fb

// Simple function with multiple arguments and type annotations
let triangle (a:float) (b:float) = 
  sqrt ((pown a 2) + (pown b 2))

triangle 3.0 4.0

// Using F# sequence expressions to work with data
let diffs = [ for v in fb -> v - avg ]

// --------------------------------------------------------
// TASK #2a: Calculate the standard deviation of FB prices
// TASK #2b: Write a function 'sdv' that takes a parameter

// (...)

// --------------------------------------------------------
// More advanced sequence expressions 

let avg = Seq.average aapl

// More explicit way of writin the previous
// (a lot more powerful - we can filter, etc.)
let diffs = 
  [ for v in aapl do 
      yield v - avg ]

// Count number of days when price is above average
let more = 
  [ for v in aapl do
      if v > avg then yield v ]
Seq.length more  

// TASK #3: Compare the days when price is above/below avg

// --------------------------------------------------------
// More functions for processing sequences

// Sort & reverse the sequence
let sorted = Seq.rev (Seq.sort fb)
Plot.Line(sorted, name="Sorted")
Plot.Line(fb, name="Normal")
Plot.Clear()

// Take first and last elements
Seq.nth 0 fb
Seq.nth ((Seq.length fb) - 1) fb

// TASK #4a: Calculate the median of the Facebook prices
// TASK #4b: Write a function & use it on Apple data too!

// (...)

// Get values as sequence of pairs (previous and next day price)
let pairs = Seq.pairwise fb
[ for (prev, next) in pairs do
    yield (prev + next) / 2.0 ]

// TASK #5: Calculate how many times did the price
// go up/down between the two consequent days

// --------------------------------------------------------

// Download MSFT and YHOO prices & draw them in a single chart
let yhoo = Yahoo.GetPrices("YHOO", from = DateTime(2012, 1, 1))
let msft = Yahoo.GetPrices("MSFT", from = DateTime(2012, 1, 1))

Plot.Line(yhoo, name="Yhoo", range=(10.0, 35.0))
Plot.Line(msft, name="Msft", range=(10.0, 35.0))
Plot.Clear()

// TASK #6 (BONUS): The Seq.windowed function generalizes 
// Seq.pairwise and returns windows of any given size 
// (As sequences - so you get sequence of sequences!)

// TASK #6a: Use the function to calculate floating average 
// over 10 days and plot that together with the origianl values

// TASK #6b: Use the function & your previous 'sdv' to 
// show the price with two more lines showing value +/- sdv