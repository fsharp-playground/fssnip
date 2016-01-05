module WithoutUnits = 

  // SAMPLE #1
    
  /// Conversion rate representing 1 EUR in GBP
  let rateEurGbp = 0.783

  /// Converts amount in EUR to GBP
  let euroToPounds eur = eur * rateEurGbp 
  /// Converts amount in GBP to EUR
  let poundsToEuro gbp = gbp / rateEurGbp


  // SAMPLE #2

  // Convert GBP 1000 to EUR
  let gbp = euroToPounds 1000.0

  // Convert EUR back to GBP
  let eur = poundsToEuro gbp


  // SAMPLE #3
  
  // We can test that converting to a currency 
  // and back returns the same amount
  let x = 1000.0
  // Because of rounding errors, this does not work:
  x = euroToPounds (poundsToEuro x)


  // SAMPLE #4

  // Helper function that tests for approximate equality
  // (but doing this with money is probably a bad idea!)
  let similar a b = 
    let d = (a + b) / 20000.0
    (a - d < b) && (a + d > b)

  // Okay, the value is the same
  similar x (euroToPounds (poundsToEuro x))


module WithDecimals = 

  // SAMPLE #5
  // In reality, we should probably use decimals 
  // (they do not lose precision for small numbers)
 
  /// Conversion rate representing 1 EUR in GBP
  let rateEurGbp = 0.783M

  /// Converts amount in EUR to GBP
  let euroToPounds eur = eur * rateEurGbp 
  /// Converts amount in GBP to EUR
  let poundsToEuro gbp = gbp / rateEurGbp

  let x = 1000.0M
  // When we use decimals, we do not get rounding errors!
  x = euroToPounds (poundsToEuro x)

module WithUnits = 

  // SAMPLE #6
  // Make it safer - let's use units of measure with decimals

  [<Measure>] type EUR
  [<Measure>] type GBP
    
  /// Conversion rate representing 1 EUR in GBP
  let rateEurGbp = 0.783M<GBP/EUR>

  /// Converts amount in EUR to GBP
  let euroToPounds (eur:decimal<EUR>) = eur * rateEurGbp 
  /// Converts amount in GBP to EUR
  let poundsToEuro gbp = gbp / rateEurGbp

  // NOTE: I added type annotations to the first function, but not to 
  // the second one. In the first case, we get EUR -> GBP function, but
  // F# infers more general type (for the second function), which
  // allows us to use it on other arguments than just GBPs. This is 
  // probably not what we want in this case, but it is worht explaining.

  // SAMPLE #7

  // Convert GBP 1000 to EUR
  let eur = poundsToEuro 1000.0M<GBP>

  // Convert EUR back to GBP
  let gbp = euroToPounds eur

  // NOTE: Type inference works nicely and 'eur' is in EUR again!

  // SAMPLE #8 (Advanced!)

  // Helper function that tests for approximate equality
  // (but doing this with money is probably a bad idea!)
  let similar (a:decimal<_>) (b:decimal<_>) = 
    let d = (a + b) / 20000.0M
    (a - d < b) && (a + d > b)

  // NOTE: To write a function that is generic over units, we need 
  // to write the type annotation as 'decimal<_>' (or 'float<_>' etc.)

  // Okay, the value is the same
  let x = 69556.79M<GBP>
  similar x (euroToPounds (poundsToEuro x)) // true
  x = (euroToPounds (poundsToEuro x))       // false


  // SAMPLE #9 (Advanced!)
  // We could even write an automated test function

  let test (unit:decimal<_>) convertThere convertBack =
    let rnd = new System.Random()
    for x in 0 .. 10000 do
      let amount = decimal (rnd.Next(10000000)) * 0.01M * unit
      if not (similar amount (convertBack (convertThere amount))) then
        failwithf "Test failed for %A" amount
      
  // We can test that converting to a currency 
  // and back returns the same amount
  test 1.0M<GBP> poundsToEuro euroToPounds
