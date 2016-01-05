#r "FSharp.PowerPack"
#r "System.Xml"
#r "System.Xml.Linq"

open System
open System.Diagnostics
open System.IO
open System.Net
open System.Xml.Linq
open System.Xml.XPath
open Microsoft.FSharp.Control.WebExtensions

let loadUrl (url:string) = async {
    let request = WebRequest.Create(url)
    use! response = request.AsyncGetResponse()
    use stream = response.GetResponseStream()
    use reader = new StreamReader(stream)
    return reader.ReadToEnd() }

let (!!) xn = XName.Get(xn)

let getTestUrls (sitemapUrl:string) = async {
    let! sitemap = loadUrl sitemapUrl
    let doc = XDocument.Parse(sitemap, LoadOptions.None)
    let locs = doc.Root.Descendants(!!"{http://www.sitemaps.org/schemas/sitemap/0.9}loc")
    let urls = seq { for loc in locs do yield loc.Value }
    return urls }

let getAllTestUrls sitemapUrls =
    seq { for sitemapUrl in sitemapUrls do yield! getTestUrls sitemapUrl |> Async.RunSynchronously }

let testUrl url = async {
    let sw = Stopwatch.StartNew()
    try
        let! page = loadUrl url
        sw.Stop()
        if page.Contains("404") || page.Contains("Site Feedback") then
            return url + " failed to load correctly in " + sw.ElapsedMilliseconds.ToString() + " ms"
        else
            return url + " loaded successfully in " + sw.ElapsedMilliseconds.ToString() + " ms"
    with
    | :? WebException as exn -> sw.Stop()
                                return url + " raised an exception after " + sw.ElapsedMilliseconds.ToString() + ": " + exn.Message }

let run host =
    printfn "Running tests (this may take awhile) ..."
    let stopwatch = Stopwatch()
    stopwatch.Start()
    
    let results =
        [| host + "/sitemap-0.xml"
           host + "/sitemap-1.xml"
           host + "/sitemap-2.xml" |]
        |> getAllTestUrls
        |> Seq.map testUrl
        |> Async.Parallel
        |> Async.RunSynchronously
    
    stopwatch.Stop()
    printfn "Checked %i urls in %d seconds" (results |> Seq.length) (stopwatch.Elapsed.Seconds)
        
    // Print all request results
    results |> Seq.iter (printfn "%s")

//    // Print the bad requests
//    let badUrl (result:string) = result.EndsWith("failed to load correctly") ||
//                                 result.Contains("raised an exception")
//    let badRequests = results |> Seq.filter badUrl
//    
//    badRequests |> Seq.iter (printfn "%s")
//    printfn "Found %i bad requests" (badRequests |> Seq.length)
    
    printfn "Finished!"

run "http://your.site.com"