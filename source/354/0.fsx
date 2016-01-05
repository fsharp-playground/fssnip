open System.Text.RegularExpressions

type Date = System.DateTime
let date d = System.DateTime.Parse(d)

type Period = { startDate:Date; endDate:Date }
type Calendar = { weekendDays:System.DayOfWeek Set; holidays:Date Set }
type Tenor = { years:int; months:int; days:int }

let tenor t =
    let regex s = new Regex(s)
    let pattern = regex ("(?<weeks>[0-9]+)W" + 
                            "|(?<years>[0-9]+)Y(?<months>[0-9]+)M(?<days>[0-9]+)D" +
                            "|(?<years>[0-9]+)Y(?<months>[0-9]+)M" + 
                            "|(?<months>[0-9]+)M(?<days>[0-9]+)D" +
                            "|(?<years>[0-9]+)Y" +
                            "|(?<months>[0-9]+)M" +
                            "|(?<days>[0-9]+)D")
    let m = pattern.Match(t)
    if m.Success then
            { new Tenor with 
                years = (if m.Groups.["years"].Success 
                         then int m.Groups.["years"].Value 
                         else 0)
                and months = (if m.Groups.["months"].Success 
                              then int m.Groups.["months"].Value 
                              else 0) 
                and days = (if m.Groups.["days"].Success 
                            then int m.Groups.["days"].Value 
                            else if m.Groups.["weeks"].Success 
                                 then int m.Groups.["weeks"].Value * 7 
                                 else 0) }
    else
        failwith "Invalid tenor format. Valid formats include 1Y 3M 7D 2W 1Y6M, etc"

let offset tenor (date:Date) = 
    date.AddDays(float tenor.days)
        .AddMonths(tenor.months)
        .AddYears(tenor.years)

let findNthWeekDay n weekDay (date:Date) =
    let mutable d = new Date(date.Year, date.Month, 1)
    while d.DayOfWeek <> weekDay do d <- d.AddDays(1.0)
    for i = 1 to n - 1 do d <- d.AddDays(7.0)
    if d.Month = date.Month then
        d
    else
        failwith "No such day"

// Assume ACT/360 day count convention
let Actual360 period = double (period.endDate - period.startDate).Days / 360.0

let rec schedule frequency period =
    seq {
        yield period.startDate
        let next = frequency period.startDate
        if (next <= period.endDate) then
            yield! schedule frequency { startDate = next; endDate = period.endDate }
    }
    
let semiAnnual (from:Date) = from.AddMonths(6)

let isBusinessDay (date:Date) calendar = 
    not (calendar.weekendDays.Contains date.DayOfWeek || calendar.holidays.Contains date)

type RollRule =
    | Actual = 0
    | Following = 1
    | Previous = 2
    | ModifiedFollowing = 3
    | ModifiedPrevious = 4

let dayAfter (date:Date) = date.AddDays(1.0)
let dayBefore (date:Date) = date.AddDays(-1.0)

let deriv f x =
    let dx = (x + max (1e-6 * x) 1e-12)
    let fv = f x
    let dfv = f dx
    if (dx <= x) then
        (dfv - fv) / 1e-12
    else
        (dfv - fv) / (dx - x)

// Newton's method with separate functions for f and df
let newton f (guess:double) = 
    guess - f guess / deriv f guess

// Simple recursive solver for Newton's method with separate functions for f and df, to a given accuracy
let rec solveNewton f accuracy guess =
    let root = (newton f guess)
    if abs(root - guess) < accuracy then root else solveNewton f accuracy root

// Assume Log-Linear interpolation
let logarithmic (sampleDate:Date) highDp lowDp = 
    let (lowDate:Date), lowFactor = lowDp
    let (highDate:Date), highFactor = highDp
    lowFactor * 
        ((highFactor / lowFactor) ** 
            (double (sampleDate - lowDate).Days / double (highDate - lowDate).Days))

let rec roll rule calendar date =
    if isBusinessDay date calendar then
        date
    else
        match rule with
        | RollRule.Actual -> date
        | RollRule.Following -> dayAfter date |> roll rule calendar
        | RollRule.Previous -> dayBefore date |> roll rule calendar
        | RollRule.ModifiedFollowing ->
            let next = roll RollRule.Following calendar date
            if next.Month <> date.Month then
                roll RollRule.Previous calendar date
            else
                next
        | RollRule.ModifiedPrevious ->
            let prev = roll RollRule.Previous calendar date
            if prev.Month <> date.Month then
                roll RollRule.Following calendar date
            else
                prev
        | _ -> failwith "Invalid RollRule"

let rec rollBy n rule calendar (date:Date) =
    match n with
    | 0 -> date
    | x -> 
        match rule with
        | RollRule.Actual -> date.AddDays(float x)

        | RollRule.Following -> dayAfter date
                                |> roll rule calendar
                                |> rollBy (x - 1) rule calendar

        | RollRule.Previous -> roll rule calendar date
                                |> dayBefore
                                |> roll rule calendar
                                |> rollBy (x - 1) rule calendar

        | RollRule.ModifiedFollowing -> 
            // Roll n-1 days Following
            let next = rollBy (x - 1) RollRule.Following calendar date
                    
            // Roll the last day ModifiedFollowing
            let final = roll RollRule.Following calendar (dayAfter next)
            if final.Month <> next.Month then
                roll RollRule.Previous calendar next
            else
                final

        | RollRule.ModifiedPrevious -> 
            // Roll n-1 days Previous
            let next = rollBy (x - 1) RollRule.Previous calendar date
                    
            // Roll the last day ModifiedPrevious
            let final = roll RollRule.Previous calendar (dayAfter next)
            if final.Month <> next.Month then
                roll RollRule.Following calendar next
            else
                final

        | _ -> failwith "Invalid RollRule"

let rec findDf interpolate sampleDate =
    function
        // exact match
        (dpDate:Date, dpFactor:double) :: tail when dpDate = sampleDate
        -> dpFactor
                    
        // falls between two points - interpolate    
        | (highDate:Date, highFactor:double) :: (lowDate:Date, lowFactor:double) :: tail 
            when lowDate < sampleDate && sampleDate < highDate
        -> interpolate sampleDate (highDate, highFactor) (lowDate, lowFactor)
              
        // recurse      
        | head :: tail -> findDf interpolate sampleDate tail
              
        // falls outside the curve
        | [] -> failwith "Outside the bounds of the discount curve"

let findPeriodDf period discountCurve = 
    let payDf = findDf logarithmic period.endDate discountCurve
    let valueDf = findDf logarithmic period.startDate discountCurve
    payDf / valueDf

let computeDf dayCount fromDf toQuote =
    let dpDate, dpFactor = fromDf
    let qDate, qValue = toQuote
    (qDate, dpFactor * (1.0 / 
                        (1.0 + qValue * dayCount { startDate = dpDate; 
                                                    endDate = qDate })))

// Just to compute f(guess)
let computeSwapDf dayCount spotDate swapQuote discountCurve swapSchedule (guessDf:double) =
    let qDate, qQuote = swapQuote
    let guessDiscountCurve = (qDate, guessDf) :: discountCurve 
    let spotDf = findDf logarithmic spotDate discountCurve
    let swapDf = findPeriodDf { startDate = spotDate; endDate = qDate } guessDiscountCurve
    let swapVal =
        let rec _computeSwapDf a spotDate qQuote guessDiscountCurve =
            function
                swapPeriod :: tail ->
                let couponDf = findPeriodDf { startDate = spotDate; 
                                              endDate = swapPeriod.endDate } guessDiscountCurve
                _computeSwapDf (couponDf * (dayCount swapPeriod) * qQuote + a) 
                    spotDate qQuote guessDiscountCurve tail

                | [] -> a
        _computeSwapDf -1.0 spotDate qQuote guessDiscountCurve swapSchedule
    spotDf * (swapVal + swapDf)

[<Measure>] type bp
[<Measure>] type percent
[<Measure>] type price
        
let convertPercentToRate (x:float<percent>) = x / 100.0<percent>
let convertPriceToRate (x:float<price>) = (100.0<price> - x) / 100.0<price>
        
type InterestRateQuote =
    | Rate of float
    | Percent of float<percent>
    | BasisPoints of float<bp>
    with
        member x.ToRate() =
            match x with
            | Rate r -> r
            | Percent p -> p / 100.0<percent>
            | BasisPoints bp -> bp / 10000.0<bp>
                    
        member x.ToPercentage() =
            match x with
            | Rate r -> r * 100.0<percent>
            | Percent p -> p
            | BasisPoints bp -> bp / 100.0<bp/percent>
                    
        member x.ToBasisPoints() =
            match x with
            | Rate r -> r * 10000.0<bp>
            | Percent p -> p * 100.0<bp/percent>
            | BasisPoints bp -> bp
    end
                
type FuturesContract = Date
let contract d = date d

type QuoteType =
| Overnight                     // the overnight rate (one day period)
| TomorrowNext                  // the one day period starting "tomorrow"
| TomorrowTomorrowNext          // the one day period starting the day after "tomorrow"
| Cash of Tenor                 // cash deposit period in days, weeks, months
| Futures of FuturesContract    // year and month of futures contract expiry
| Swap of Tenor                 // swap period in years

// Bootstrap the next discount factor from the previous one
let rec bootstrap dayCount quotes discountCurve =
    match quotes with
        quote :: tail -> 
        let newDf = computeDf dayCount (List.head discountCurve) quote
        bootstrap dayCount tail (newDf :: discountCurve)
        | [] -> discountCurve

// Generate the next discount factor from a fixed point on the curve 
// (cash points are wrt to spot, not the previous df)
let rec bootstrapCash dayCount spotDate quotes discountCurve =
    match quotes with
        quote :: tail -> 
        let spotDf = (spotDate, findDf logarithmic spotDate discountCurve)
        let newDf = computeDf dayCount spotDf quote
        bootstrapCash dayCount spotDate tail (newDf :: discountCurve)
        | [] -> discountCurve

let bootstrapFutures dayCount futuresStartDate quotes discountCurve =
    match futuresStartDate with
    | Some d ->
        bootstrap dayCount
                    (Seq.toList quotes) 
                    ((d, findDf logarithmic d discountCurve) :: discountCurve)
    | None -> discountCurve

// Swaps are computed from a schedule generated from spot and priced 
// according to the curve built thusfar
let rec bootstrapSwaps dayCount spotDate calendar swapQuotes discountCurve =
    match swapQuotes with
        (qDate, qQuote) :: tail ->
        // build the schedule for this swap                
        let swapDates = schedule semiAnnual { startDate = spotDate; endDate = qDate }
        let rolledSwapDates = Seq.map (fun (d:Date) -> roll RollRule.Following calendar d) 
                                        swapDates
        let swapPeriods = Seq.toList (Seq.map (fun (s, e) -> 
                                                { startDate = s; 
                                                  endDate = e }) (Seq.pairwise rolledSwapDates))
                
        // solve
        let accuracy = 1e-12
        let spotFactor = findDf logarithmic spotDate discountCurve
        let f = computeSwapDf dayCount spotDate (qDate, qQuote) discountCurve swapPeriods
        let newDf = solveNewton f accuracy spotFactor

        bootstrapSwaps dayCount spotDate calendar tail ((qDate, newDf) :: discountCurve)
        | [] -> discountCurve

let USD = { weekendDays = Set [ System.DayOfWeek.Saturday; System.DayOfWeek.Sunday ]; 
            holidays = Set [ date "2009-01-01"; 
                             date "2009-01-19"; 
                             date "2009-02-16"; 
                             date "2009-05-25"; 
                             date "2009-07-03"; 
                             date "2009-09-07"; 
                             date "2009-10-12"; 
                             date "2009-11-11"; 
                             date "2009-11-26"; 
                             date "2009-12-25" ] }
        
let curveDate = date "2009-05-01"
let spotDate = rollBy 2 RollRule.Following USD curveDate

let quotes = [ (Overnight, 0.045);
               (TomorrowNext, 0.045);
               (Cash (tenor "1W"), 0.0462);
               (Cash (tenor "2W"), 0.0464);
               (Cash (tenor "3W"), 0.0465);
               (Cash (tenor "1M"), 0.0467);
               (Cash (tenor "3M"), 0.0493);
               (Futures (contract "Jun2009"), 95.150);
               (Futures (contract "Sep2009"), 95.595);
               (Futures (contract "Dec2009"), 95.795);
               (Futures (contract "Mar2010"), 95.900);
               (Futures (contract "Jun2010"), 95.910);
               (Swap (tenor "2Y"), 0.04404);
               (Swap (tenor "3Y"), 0.04474);
               (Swap (tenor "4Y"), 0.04580);
               (Swap (tenor "5Y"), 0.04686);
               (Swap (tenor "6Y"), 0.04772);
               (Swap (tenor "7Y"), 0.04857);
               (Swap (tenor "8Y"), 0.04924);
               (Swap (tenor "9Y"), 0.04983);
               (Swap (tenor "10Y"), 0.0504);
               (Swap (tenor "12Y"), 0.05119);
               (Swap (tenor "15Y"), 0.05201);
               (Swap (tenor "20Y"), 0.05276);
               (Swap (tenor "25Y"), 0.05294);
               (Swap (tenor "30Y"), 0.05306) ]

let spotPoints = quotes
                    |> List.choose (fun (t, q) -> 
                        match t with
                        | Overnight _ -> Some (rollBy 1 RollRule.Following USD curveDate, q)
                        | TomorrowNext _ -> Some (rollBy 2 RollRule.Following USD curveDate, q)
                        | TomorrowTomorrowNext _ -> Some (rollBy 3 RollRule.Following USD curveDate, q)
                        | _ -> None)
                    |> List.sortBy (fun (d, _) -> d)

let cashPoints = quotes
                    |> List.choose (fun (t, q) -> 
                        match t with
                        | Cash c -> Some (offset c spotDate |> roll RollRule.Following USD, q)
                        | _ -> None)
                    |> List.sortBy (fun (d, _) -> d)

let futuresQuotes = quotes
                    |> List.choose (fun (t, q) -> 
                        match t with
                        | Futures f -> Some (f, q)
                        | _ -> None)
                    |> List.sortBy (fun (c, _) -> c)
                                
let (sc, _) = List.head futuresQuotes
let (ec, _) = futuresQuotes.[futuresQuotes.Length - 1]   
let futuresStartDate = findNthWeekDay 3 System.DayOfWeek.Wednesday sc 
                        |> roll RollRule.ModifiedFollowing USD
let futuresEndDate = (new Date(ec.Year, ec.Month, 1)).AddMonths(3)

// "invent" an additional contract to capture the end of the futures schedule
let endContract = (futuresEndDate, 0.0)
            
let futuresPoints = Seq.append futuresQuotes [endContract]
                    |> Seq.pairwise
                    |> Seq.map (fun ((_, q1), (c2, _)) -> 
                        (findNthWeekDay 3 System.DayOfWeek.Wednesday c2 
                            |> roll RollRule.ModifiedFollowing USD, (100.0 - q1) / 100.0))
                    |> Seq.toList            
            
let swapPoints = quotes
                    |> List.choose (fun (t, q) -> 
                        match t with
                        | Swap s -> Some (offset s spotDate |> roll RollRule.Following USD, q)
                        | _ -> None)
                    |> List.sortBy (fun (d, _) -> d)

let discountFactors = [ (curveDate, 1.0) ]
                        |> bootstrap Actual360 spotPoints 
                        |> bootstrapCash Actual360 spotDate cashPoints
                        |> bootstrapFutures Actual360 (Some futuresStartDate) futuresPoints
                        |> bootstrapSwaps Actual360 spotDate USD swapPoints
                        |> Seq.sortBy (fun (qDate, _) -> qDate)

printfn "Discount Factors"
Seq.iter (fun (d:Date, v) -> printfn "\t%s\t%.13F" (d.ToString("yyyy-MM-dd")) v) discountFactors
            
let zeroCouponRates = discountFactors 
                      |> Seq.map (fun (d, f) 
                                    -> (d, 100.0 * -log(f) * 365.25 / double (d - curveDate).Days))

printfn "Zero-Coupon Rates"
Seq.iter (fun (d:Date, v) -> printfn "\t%s\t%.13F" (d.ToString("yyyy-MM-dd")) v) zeroCouponRates
