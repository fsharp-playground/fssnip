open System
open System.Data
open System.Linq
open System.Net
open System.Text.RegularExpressions
open System.Threading

let querygooglen maxn (milli:int) (q:string) dayback =
    use client = new WebClient()       
    client.Headers.["User-Agent"] <- "Lynx/2.8.7rel.1 libwww-FM/2.14 SSL-MM/1.4.1 OpenSSL/1.0.1c"    
    [0..100..maxn] 
    |> List.collect (fun x -> 
                    printfn "Query %d" x                                    
                    Thread.Sleep(milli)                                    
                    let str1 = "http://www.google.com/search?q="+q.ToString()+"&hl=en&start="+x.ToString()+"&num=100"
                    let q = if dayback > 0 then
                                let tend = DateTime.Now.ToString("MM/dd/yyyy")
                                let tbeg = DateTime.Now.AddDays( float(-dayback)).ToString("MM/dd/yyyy")
                                str1+"&tbs=cdr:1,cd_min:"+tend+",cd_max:"+tbeg
                            else str1
                    printfn "GOOGLE:%s" q
                    let str = client.DownloadString(q)
                    let regexurl = new  Regex (@"url\?q=([^\&]+)")
                    let rez = [ for url in regexurl.Matches(str) do 
                                    yield url.Groups.[1].Captures.[0].Value ] 
                    printfn "Returned %d Results" (rez.Count())
                    rez) 
    |> Set.ofList 
    |> Set.toList

