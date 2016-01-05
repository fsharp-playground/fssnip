#load @"J:\VS2013 Projects\...\FsLab.fsx"

open Deedle
open FSharp.Charting

(* The raw data is from: http://www.ed.ac.uk/schools-departments/geosciences/weather-station/download-weather-data *)
let last31 = Frame.ReadCsv("J:/BigData/JCMB_last31days.csv")

// To find how many days that the surface temperature is low than 2 Celsius at 2pm.
let resultDays = 
    last31
    // extract time and temperature columns
    |> Frame.getCols["time"; "temperature"]
    // filter out the time and temperature of interests
    |> Frame.filterRowValues(fun row -> row?time = 1400.0 && row?temperature < 2.0)
    // count the results
    |> Frame.countRows

// In the past 31 days, only 2 days at 2pm the temperature was lower than 2.0 Celsius.
// val resultDays : int = 2

// Time series (at 2pm)
let twoPmCol = 
    last31
    |> Frame.getSeries("time")
    |> Series.filterValues(fun s -> s = 1400.0)

Chart.Pie(twoPmCol)

// Temperatures series(higher than 12.85 Cel)
let higherTemp= 
    last31
    |> Frame.getSeries("temperature")
    |> Series.filterValues(fun c -> c > 12.85)

Chart.Pie(higherTemp)

let newFrame = Frame(["Time"; "HighTemperature"], [twoPmCol; higherTemp])

//Combine chart
Chart.Combine[
                Chart.Line(newFrame?Time |> Series.observations)
                Chart.Line(newFrame?HighTemperature |> Series.observations)
             ]

// Sample to plot using Chart.Bar
let sample = 
    last31
        // extract time and temperature columns
        |> Frame.getCols["time"; "temperature"]
        // filter out the time and temperature of interests
        |> Frame.filterRowValues(fun row -> row?time = 1400.0 && row?temperature > 0.0)

Chart.Bar((sample?temperature) |> Series.observations)
