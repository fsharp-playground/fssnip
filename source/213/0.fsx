open System.Xml.Linq

let xname str = XName.Get str

let sitemapEntry = 
    XElement(xname "url",
        XElement(xname "loc", "http://fssnip.net/"),
        XElement(xname "lastmod", "2011-03-11"),
        XElement(xname "changefreq", "daily"),
        XElement(xname "priority", 0.5)
    )

let priority = sitemapEntry.Descendants() |> Seq.tryFind (fun x -> x.Name.LocalName = "priority")