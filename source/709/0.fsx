open System
open System.Net
open System.Text

let listener (handler:(HttpListenerRequest -> HttpListenerResponse -> Async<unit>)) =
    let hl = new HttpListener()
    hl.Prefixes.Add "http://*:8080/"
    hl.Start()
    let task = Async.FromBeginEnd(hl.BeginGetContext, hl.EndGetContext)
    async {
        while true do
            let! context = task
            Async.Start(handler context.Request context.Response)
    } |> Async.Start

let output (req:HttpListenerRequest) =
    if req.UrlReferrer = null then
        "No referrer!"
    else
        "Referrer: " + req.UrlReferrer.ToString()

listener (fun req resp ->
    async {
        let txt = Encoding.ASCII.GetBytes(output req)
        resp.OutputStream.Write(txt, 0, txt.Length)
        resp.OutputStream.Close()
    })

printfn "Press return to exit..."
Console.ReadLine()
    |> ignore