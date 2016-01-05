open System
open System.IO
open System.Net
 
// Using a WebClient.
let fetch (url : string) =
    use client = new WebClient()
    let agent = "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)"
    client.Headers.Add("user-agent", agent)
    client.DownloadString url
 
// Using a WebRequest.
let fetch' (url : string) = 
    let req = WebRequest.Create url :?> HttpWebRequest
    req.UserAgent <- "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)"
    use stream = req.GetResponse().GetResponseStream()
    use reader = new StreamReader(stream)
    reader.ReadToEnd()