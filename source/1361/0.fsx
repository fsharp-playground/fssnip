open System
open System.IO
open System.Net

let hostname = "www.microsoft.com"      // This is the host name of your computer

[<EntryPoint>]
let main argv =     
    let host = Dns.GetHostEntry(hostname)
    let reverseIP = host.AddressList.[0].ToString()
    let req = WebRequest.Create("http://checkip.dyndns.org/")
    let resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    let rdr = new StreamReader(stream)
    let html = rdr.ReadToEnd()
    let first = html.IndexOf("Address: ") + 9
    let last = html.LastIndexOf("</body>")
    let IP = html.Substring(first, last - first)
    if 0 = String.Compare(reverseIP,IP) then
        printfn "Match, ip address for %s = %s" hostname IP
    else
        printfn "Addresses do not match! %s resolves to %s, but local IP address reported as %s"
          hostname reverseIP IP
    Console.ReadKey() |> ignore
    0 // return an integer exit code