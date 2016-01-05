//generic routine to calculate tax
let taxOf salary taxRates = 
    taxRates 
        |> Seq.mapi(fun i (rate, band) -> 
            let prevBand = (if i > 0 then taxRates |> Seq.nth (i-1) |> snd else 0m)
            match salary with 
                | x when x > band -> (band - prevBand) * rate
                | x when x < prevBand -> 0m
                | x -> (x - prevBand) * rate
            )
        |> Seq.sum

//define custom tax bands and rates
let israelTaxRates = [
    ( 0.1m, 5070m); 
    (0.14m, 8660m); 
    (0.23m, 14070m); 
    ( 0.3m, 21240m); 
    (0.33m, 40230m); 
    (0.45m, System.Decimal.MaxValue)]


//use currying to build a higher order function to calculate US Tax Rates
let taxOfIsrael salary = israelTaxRates |> taxOf salary
                
taxOfIsrael 10000m
