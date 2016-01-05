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
  let similar a b = 
    let d = (a + b) / 20000.0
    (a - d < b) && (a + d > b)

  // Good - the value is the same
  similar x (euroToPounds (poundsToEuro x))


module WithUnits = 

  // SAMPLE #5
  [<Measure>] type EUR
  [<Measure>] type GBP
    
  /// Conversion rate representing 1 EUR in GBP
  let rateEurGbp = 0.783<GBP/EUR>

  /// Converts amount in EUR to GBP
  let euroToPounds (eur:float<EUR>) = eur * rateEurGbp 
  /// Converts amount in GBP to EUR
  let poundsToEuro gbp = gbp / rateEurGbp

  // NOTE: I added type annotations to the first function, but not to 
  // the second one. In the first case, we get EUR -> GBP function, but
  // F# infers more general type (for the second function), which
  // allows us to use it on other arguments than just GBPs. This is 
  // probably not what we want in this case, but it is worht explaining.

  // SAMPLE #6

  // Convert GBP 1000 to EUR
  let gbp = euroToPounds 1000.0<EUR>

  // Convert EUR back to GBP
  let eur = poundsToEuro gbp

  // NOTE: Type inference works nicely and 'eur' is in EUR again!

  // We can test that converting to a currency 
  // and back returns the same amount
  let x = 1000.0<GBP>

  // Helper function that tests for approximate equality
  let similar (a:float<_>) (b:float<_>) = 
    let d = (a + b) / 20000.0
    (a - d < b) && (a + d > b)

  // NOTE: To write a funciton that is generic over units, we need
  // to add a type annotation 'float<_>'. Now we can use 'similar'
  // to compare units.

  // Good - the value is the same
  similar x (euroToPounds (poundsToEuro x))