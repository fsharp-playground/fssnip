type T = FSharp.Data.JsonProvider<"http://search.twitter.com/search.json?q=%23fsharp&lang=en&rpp=1&page=1">
let tweets (tag : string) (since : System.DateTime) =
    let enc = System.Web.HttpUtility.UrlEncode : string -> string
    let rec page n =        
        let data = T.Load(sprintf "http://search.twitter.com/search.json?q=%s&rpp=100&page=%d&since=%4d-%02d-%02d" (enc tag) n since.Year since.Month since.Day)
        seq{
            yield! data.Results
            if not (Seq.isEmpty data.Results) then yield! page (n + 1)
        }
    page 1

// usage
tweets "#fsharp" (System.DateTime.Parse("5/17/2013"))
|> Seq.iter ( fun t -> printfn "%-21O %-15s %s" t.CreatedAt t.FromUser t.Text )