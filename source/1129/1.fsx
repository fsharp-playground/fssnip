#r "System.Xml.Linq.dll"
open System
open System.Xml.Linq

// University enrollment in different countries of the world
let url = "http://api.worldbank.org/countries/indicators/SE.TER.ENRR?per_page=500&date=2005:2005"
let doc = XDocument.Load(url)

/// Creataes XML name using the WorldBank namespace
let wb name = XName.Get(name, "http://www.worldbank.org")

let (?) (pt:XElement) (name:string) : 'R =
  let value = pt.Element(wb name).Value
  try
    let conv = System.Convert.ChangeType(value, typeof<'R>)
    unbox conv
  with _ ->
    Unchecked.defaultof<'R>


for pt in doc.Root.Elements(wb "data") do
  let name = pt?country
  let value = pt?value
  printfn "%s (%f)" name value



