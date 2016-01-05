open System
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

// Run the HTTP web request
Http.RequestString
  ( "https://www.google.com/search?q=super",
    query   = [ "q", "super" ],
    headers = [ Accept HttpContentTypes.Any; UserAgent "Mozilla/5.0" ])