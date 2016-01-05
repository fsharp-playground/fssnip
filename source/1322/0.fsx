//Initialize charting libraries

open Samples.Charting.DojoChart

//Load World Bank Type Provider
#r "Samples.WorldBank.dll"

//Get data context
let data = Samples.WorldBank.GetDataContext()
//Set up list of countries
let countries = 
  [ data.Countries.``Pakistan``; 
    data.Countries.China;
    data.Countries.India; 
    data.Countries.``United States`` ]

//Plot total population of selected countries
Chart.Combine([ for c in countries -> Chart.Line (c.Indicators.``Population, total``, Name=c.Name) ])
     .AndTitle("Population, 1960-2012")
