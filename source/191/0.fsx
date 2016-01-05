open System
open System.IO
open System.Net
open System.Threading
open System.Windows.Forms
open SHDocVw
open mshtml
open System.Windows.Forms
open System.Text.RegularExpressions

let (|Match|_|) (pat:string) (inp:string) =
    let m = Regex.Match(inp, pat) in
    if m.Success
    then Some (List.tail [ for g in m.Groups -> g.Value ])
    else None

let (|Matches|_|) (pat:string) (inp:string) =
    let m = Regex.Matches(inp, pat) in
    if m.Count > 0
    then Some ( [ for g in m -> g.Value ])
    else None



let getPage (url:string) = 
        let (html:HtmlDocument ref) =  ref null

        let handler (sender:obj) (e: WebBrowserDocumentCompletedEventArgs) = 
            let wb = sender :?> (WebBrowser)
            html := (wb.Document)

        use wb = new WebBrowser()
        wb.Visible<-true
        wb.DocumentCompleted.Add(handler wb)
        wb.Navigate(url)

        while wb.ReadyState <> WebBrowserReadyState.Complete do
                    Application.DoEvents()
    
        html

let googleSearch term = 
    let rec googleSearch link pass res = 
        let search = getPage link
        Thread.Sleep(TimeSpan.FromSeconds(5.0))
    
        let potentialLinks = 
            seq{ for link in search.Value.Links do
                     if not(link.OuterHtml.Contains("google")) then 
                         yield link.OuterHtml }
            |>Seq.toArray
   
        let pat = "href=\"([^>]+)\">"

        let links = 
             potentialLinks
             |> Array.filter(fun elem -> elem.Contains("<A class=\"l\" onmousedown="))
             |> Array.map( fun elem -> match elem with 
                                       |Match pat (link::t) -> link
                                       | _ -> failwith "critical parsing error")
        let next = 
             potentialLinks
             |> Array.filter(fun elem -> elem.Contains("<A class=\"fl\"")&& elem.Contains("/search?"))
             |> Array.map( fun elem -> match elem with 
                                       | Match pat (link::t) -> "http://www.google.com"+link
                                       | _ -> failwith "critical parsing error")

        let newRes = res|>Array.append(links)
        
        if (next.Length-1) <= pass then
           newRes
        else
           let newPass = pass+1
           googleSearch ((next.[newPass]).Replace(";","&")) newPass newRes

    googleSearch ("http://www.google.com/search?num=100&hl=en&q="+term+"&btnG=Search&aq=f&aqi=&aql=&oq=") -1 [||]   


googleSearch "F# love" 