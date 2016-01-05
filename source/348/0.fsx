//#r "System.Runtime.Serialization"
//#r "FSharp.PowerPack"

open Microsoft.FSharp.Control.WebExtensions
open System
open System.Runtime.Serialization
open System.Runtime.Serialization.Json
open System.Net
open System.IO

let internal fetchAsync (url : Uri) trigger = 
    let req = WebRequest.CreateHttp url
    req.CookieContainer <- new CookieContainer()
    let asynccall =
        async{
            try
                let! res = req.AsyncGetResponse() 
                use stream = res.GetResponseStream()
                use reader = new StreamReader(stream)
                let! rdata = reader.AsyncReadToEnd()                             
                callresult <- rdata //some processing like unjson here...
                trigger "" |> ignore
            with
                | _ as ex -> //for debug
                    failwith(ex.ToString()) 
        }

    asynccall |> Async.StartImmediate


//UI-thread syncronization with Dispatcher
let trigger _ = 
    let update _ = x.MyViewModelProperty <- callresult
    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(new Action(update)) |> ignore
let service = new Uri("http://...", UriKind.Absolute)
fetchAsync service trigger
