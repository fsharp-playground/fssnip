//generic routine to calculate tax
let taxOf salary taxRates = 
    ((0m,0)::taxRates, taxRates) 
        ||> Seq.zip 
         |> Seq.map(fun ((_, prevBand),(rate, band)) -> (prevBand, rate, band))
         |> Seq.sumBy(fun (prevBand, rate, band) -> 
            match salary with 
                | x when x < prevBand -> 0m
                | x when x > band -> decimal(band - prevBand) * rate
                | x -> decimal(x - prevBand) * rate
            )

//define custom tax bands and rates
let israelTaxRates = [
    ( 0.1m, 5070); 
    (0.14m, 8660); 
    (0.23m, 14070); 
    ( 0.3m, 21240); 
    (0.33m, 40230); 
    (0.45m, System.Int32.MaxValue)]

//use currying to build a higher order function to calculate US Tax Rates
let taxOfIsrael salary = israelTaxRates |> taxOf salary
                
taxOfIsrael 10000
