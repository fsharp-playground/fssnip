// Just read this article about consuming the Stackoverflow API in powershell: 
// http://www.dougfinke.com/blog/index.php/2012/02/08/using-powershell-v3-to-consume-stackoverflow-json-api/
// and ported it to script-ish F# 
// using ideas from http://stackoverflow.com/a/4208663/21239

#r "System.Web.Extensions"

open System

let (?) (o:obj) (name:obj) = 
    match o with
    | :? Collections.Generic.Dictionary<_,_> as d -> d.[string name]
    | :? Array as a -> (a.GetValue(name :?> int))

let getStackOverflowUser id =
    let url = sprintf "http://api.stackoverflow.com/1.1/users/%d" id
    let request = Net.WebRequest.Create url :?> Net.HttpWebRequest
    request.AutomaticDecompression <- Net.DecompressionMethods.GZip
    use response = request.GetResponse()
    use rs = response.GetResponseStream()
    use reader = IO.StreamReader rs
    (Web.Script.Serialization.JavaScriptSerializer()).DeserializeObject(reader.ReadToEnd())

(getStackOverflowUser 22656)?users?(0)?display_name