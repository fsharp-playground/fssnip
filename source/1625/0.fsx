module CronSchedule

open System
open System.Text.RegularExpressions

let cronRegex = 
    Regex(@"(?<=^|,)(?<rangeStart>(?:\d+|\*))(?:\-(?<rangeEnd>(?:\d+|\*)))?(?:\/(?<devided>[1-9]\d*))?(?=,|$)")

type CronSchedule = {
    minutes: Set<int>
    hours: Set<int>
    dayOfMonth: Set<int>
    months: Set<int>
    dayOfWeek: Set<int>
    fail: string
    original: string
} with 
    static member Fail(str, original) = 
        { minutes = Set.empty; hours = Set.empty; dayOfMonth = Set.empty; 
            months = Set.empty; dayOfWeek = Set.empty; fail = str; original = original}
    member x.IsTime(date: DateTime) = 
        x.minutes.Contains(date.Minute) && 
        x.hours.Contains(date.Hour) &&
        x.dayOfMonth.Contains(date.Day) &&
        x.months.Contains(date.Month) &&
        x.dayOfWeek.Contains(int date.DayOfWeek)
    member x.NextTime(startDate: DateTime, ?maxDate: DateTime) = 
        let maxDate = defaultArg maxDate (startDate.AddYears(1))
        let years = [startDate.Year..maxDate.Year]  
        let startMonthes, monthes = 
            if Set.isEmpty x.months then 
                [yield![startDate.Month..12];yield![1..startDate.Month-1]], [1..12]
            else x.months |> Set.toList |> fun x -> x,x
        let startDaysOfMonth, daysOfMonth = 
            if Set.isEmpty x.dayOfMonth then 
                [yield![startDate.Day..31];yield![1..startDate.Day-1]], [1..31]
            else x.dayOfMonth |> Set.toList |> fun x -> x,x 
        let startHours, hours = 
            if Set.isEmpty x.hours then 
                [yield![startDate.Hour..23];yield![0..startDate.Hour-1]], [0..23]
            else x.hours |> Set.toList |> fun x -> x,x
        let startMinutes, minutes = 
            if Set.isEmpty x.minutes then 
                [yield![startDate.Minute..59];yield![0..startDate.Minute-1]], [0..59]
            else x.minutes |> Set.toList |> fun x -> x,x
        let daysOfWeek = 
            if Set.isEmpty x.dayOfWeek then Set [0..7]
            else x.dayOfWeek
        let rec search = 
            function 
            //no more years to check
            | [], _, _, _, _ -> None
            //no more monthes to check -> go to next year
            | year::avYears, [], _, _, _ -> 
                search (avYears, monthes, daysOfMonth, hours, minutes)
            //no more days of monthes to check -> go to next month
            | avYears, month::avMonthes, [], _, _ -> 
                search (avYears, avMonthes, daysOfMonth, hours, minutes)
            // days count in this month year is less than specified day of month --> go to next month
            | (year::_ as avYears), month::avMonthes, dayOfMonth::_, _, _ 
                when DateTime.DaysInMonth(year, month) < dayOfMonth -> 
                search (avYears, avMonthes, daysOfMonth, hours, minutes)
            //no more hours -> go to next day
            | avYears, avMonthes, dayOfMonth::avDaysOfMonth, [], _ -> 
                search (avYears, avMonthes, avDaysOfMonth, hours, minutes)
            // no more minutes -> go to next hour
            | avYears, avMonthes, avDaysOfMonth, hour::avHours, [] -> 
                search (avYears, avMonthes, avDaysOfMonth, avHours, minutes)
            //build date and additional check
            | (year::_ as avYears), (month::_ as avMonthes), 
                (dayOfMonth::otherDaysOfMonth as avDaysOfMonth), 
                (hour::_ as avHours), minute::avMinutes ->
                let date = DateTime(year, month, dayOfMonth, hour, minute, 0)
                if date >= startDate && date <= maxDate then
                    if daysOfWeek.Contains(int date.DayOfWeek) then Some date
                    else search (avYears, avMonthes, otherDaysOfMonth, hours, minutes)
                else search (avYears, avMonthes, avDaysOfMonth, avHours, avMinutes)
        search (years, startMonthes, startDaysOfMonth, startHours, startMinutes)

let (|Cron|_|) (rangeStart, rangeEnd) str = 
    let res = cronRegex.Matches(str)
    if res.Count > 0 then 
        [ 
            for m in res do
                let rangeStartGroup = m.Groups.["rangeStart"]
                let rangeEndGroup = m.Groups.["rangeEnd"]
                let stepGroup = m.Groups.["devided"]
                let step = 
                    if stepGroup.Success then
                        Int32.Parse(stepGroup.Value)
                    else 1 //defaultStep
                match rangeStartGroup.Success, rangeEndGroup.Success with
                | true, false when rangeStartGroup.Value = "*" -> ()
                | true, false ->  yield Int32.Parse(rangeStartGroup.Value)
                | true, true when rangeStartGroup.Value = "*" && rangeEndGroup.Value = "*" -> ()
                | true, true ->
                    let rangeStart = 
                        if rangeStartGroup.Value = "*" then rangeStart
                        else max rangeStart (Int32.Parse(rangeStartGroup.Value))
                    let rangeEnd = 
                        if rangeEndGroup.Value = "*" then rangeEnd
                        else min rangeEnd (Int32.Parse(rangeEndGroup.Value))
                    for i in rangeStart..step..rangeEnd do
                        yield i
                | _, _ -> ()
        ] |> Set.ofList |> Some
    else None

let create(expression: string) = 
    let parts = expression.Split() 
    match parts with
    | [| Cron (0,59) minutes; Cron (0,23) hours; Cron (1,31) days; 
            Cron (1,12) months; Cron (0,7) daysOfWeek |] -> 
        { minutes = minutes; hours = hours; dayOfMonth = days; months = months; 
            dayOfWeek = (if Set.contains 7 daysOfWeek then Set.add 0 daysOfWeek else daysOfWeek); 
            fail = ""; original = expression}
    | _ -> CronSchedule.Fail("Wrong expression format. Expression should contain 5 parts", expression)

(*Examples:  
let a = CronSchedule.create "40-59/3,15-20,24,28-35/2 11-17 * * 0,3,5"
let b = CronSchedule.create "*/5 * * * *"
let fromTime = DateTime.UtcNow
let nextLaunch = a.NextTime(fromTime)
*)