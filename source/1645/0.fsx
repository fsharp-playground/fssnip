#r @"C:\Projects\FsharpWindowsStorePrototype\packages\FSharp.Data.2.1.0\lib\net40\FSharp.Data.dll"

open System
open FSharp.Data

type SampleHtmlProvider = HtmlProvider<"http://www.weather.com/weather/today/l/98033:4:US">
let data = SampleHtmlProvider.Load("http://www.weather.com/weather/today/l/98033:4:US")
let info = data.Tables.Table1.Rows.[0].Column1

open System
open FSharp.Data

type SampleHtmlProvider = HtmlProvider<"http://www.weather.com/weather/today/l/98033:4:US">
let data = SampleHtmlProvider.Load("http://www.weather.com/weather/today/l/98033:4:US")
let info = data.Tables.Table1.Rows.[0].Column1