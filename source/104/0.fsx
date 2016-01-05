open System

let getAge (d : DateTime) =
        let d' = DateTime.Now
        match d' > d with
        | true ->
            let months = 12 * (d'.Year - d.Year) + (d'.Month - d.Month)
 
            match d'.Day < d.Day with
            | true -> let days = DateTime.DaysInMonth(d.Year, d.Month) - d.Day + d'.Day
                      let years = (months - 1) / 12
                      let months' = (months - 1) - years * 12
                      (years, months', days)
            | false -> let days = d'.Day - d.Day
                       let years = months / 12
                       let months' = months - years * 12
                       (years, months', days)
 
        | false -> (0,0,0)

// Example
let birthDate = DateTime.Parse("2/8/1995")
let y, m, d = getAge birthDate
printfn "Age: %d years, %d months and %d days." y m d

// Output
// Age: 15 years, 5 months and 6 days. 