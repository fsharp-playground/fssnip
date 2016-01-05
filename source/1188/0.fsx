// Load Freebase data connection and charting library
#r "Samples.DataStore.Freebase.dll"
open Samples.Charting.DojoChart
open Samples.DataStore.Freebase

let freebase = FreebaseData.GetDataContext()

// ------------------------------------------------------------------
// DEMO: Querying cyclone data using F# LINQ
// ------------------------------------------------------------------

let cyclones = 
  query { for x in freebase.Commons.Meteorology.``Tropical Cyclones`` do
          where x.``Highest winds``.HasValue
          where (x.Damages.Currency.Name = "United States dollar")
          select (x.``Highest winds``.Value, x.Damages.Amount.Value / 1e9) }
  |> Seq.toList

// Plot wind speed and damage in USD as point chart
let cyclonChart =    
  Chart.Point(cyclones) 
    .WithYAxis(Title="Damage (US$)")
    .WithXAxis("Wind Speed")

// ------------------------------------------------------------------
// DEMO: Adding linear regression using Math.NET
// ------------------------------------------------------------------

// For more information see
// http://christoph.ruegg.name/blog/linear-regression-mathnet-numerics.html

#r "MathNet.Numerics.dll"
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

// Build matrix and vector representing the equation system 
let cols : Generic.Vector<float>[] = 
  [| DenseVector(cyclones.Length, 1.0)
     DenseVector([| for x, y in cyclones -> float x |]) |] 
let x = DenseMatrix.CreateFromColumns(cols)
let y = DenseVector([| for x, y in cyclones -> y |])

// QR decomposition gives us attributes of y=a+x*b line
let [| a; b |] = x.QR().Solve(y) |> Seq.toArray

// Draw chart with linear regression
Chart.Combine
  [ cyclonChart
    Chart.Line([20.0, a+b*20.0; 100.0, a+b*100.0 ]) ]
    