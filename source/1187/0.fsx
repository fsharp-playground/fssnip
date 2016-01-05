// Load charting and WorldBank type provider
open Samples.Charting.DojoChart
#r "Samples.WorldBank.dll"

// Initiate connection to the WorldBank 
let data = Samples.WorldBank.GetDataContext()

// ------------------------------------------------------------------
// DEMO: Plotting university enrollment data
// ------------------------------------------------------------------

// Get University enrollment from OECD and CZ
let oecd = data.Countries.``OECD members``.Indicators.``School enrollment, tertiary (% gross)``
let cz = data.Countries.``Czech Republic``.Indicators.``School enrollment, tertiary (% gross)``

// Pass data as arguments to chart
Chart.Line(oecd)
Chart.Line(cz)

// Alternative using F# "pipelining"
oecd |> Chart.Line
cz |> Chart.Line

let charts = [ Chart.Line(cz, Name="CZ"); Chart.Line(oecd, Name="OECD") ]
Chart.Combine(charts).WithLegend()

// ------------------------------------------------------------------
// TASK: Compare "Central government debt" of UK, USA, Greece, ...
// ------------------------------------------------------------------

// (...)