#r "System.Xml.Linq.dll"
open System
open System.Xml.Linq

// University enrollment in different countries of the world
let url = "http://api.worldbank.org/countries/indicators/SE.TER.ENRR?per_page=500&date=2005:2005"
let doc = XDocument.Load(url)

/// Creataes XML name using the WorldBank namespace
let wb name = XName.Get(name, "http://www.worldbank.org")

let (?) (pt:XElement) (s:string) =

for pt in doc.Root.Elements(wb "data") do
  let name = pt.Element(wb "country").Value
  let value = pt.Element(wb "value").Value
  printfn "%s (%f)" name (if value = "" then 0.0 else float value)



