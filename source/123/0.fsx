// [snippet:Idea #1: Imperative ala Matlab or R]
// (Simple and easy for R/Matlab users, but imperative)
plot.Subplot(2, 2)
plot.Candles(data1)
plot.Stock(data2)
plot.Stock(data3)
plot.Candles(data4)
// [/snippet]

// [snippet:Idea #2: Using custom operators]
// (Short, but the operators are difficult to discover if you 
// don't know them; Also we may need richer layout combinators)
(plot.Candles(data1) <|> plot.Stock(data2)) <->
(plot.Stock(data3) <|> plot.Candles(data4))
// [/snippet]

// [snippet:Idea #3: Compose using lists]
// (Longer and a bit ugly because of the indentation, but conceptually 
// simple and we can use comprehensions to generate charts)
plot.Subplot
 ( 2, 2, 
   [ plot.Candles(data1)
     plot.Stock(data2)
     plot.Stock(data3)
     plot.Candles(data4) ])
// [/snippet]

// [snippet:Idea #4: Computation builder based]
// (Uses tricky features, but it looks quite nice and 
// we can use comprehensions (but cannot just write a list))
plot.Subplot(2, 2) {
   yield plot.Candles(data1)
   yield plot.Stock(data2)
   yield plot.Stock(data3)
   yield plot.Candles(data4) 
}
// [/snippet]