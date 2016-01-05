//using F# Power Pack

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Net
open Microsoft.FSharp.Control.WebExtensions

let mutable callresult = ""
//let event = new Event<string>()
//let eventvalue = event.Publish

let internal fetch (url : Uri) = 
    let req = WebRequest.CreateHttp url
    req.CookieContainer <- new CookieContainer()
    let asynccall =
        async{
            try
                let! res = req.AsyncGetResponse() 
                use stream = res.GetResponseStream()
                use reader = new StreamReader(stream)
                let! txt = reader.AsyncReadToEnd()
                //event.Trigger(txt)
                callresult <- txt
            with
                | :? System.Exception as ex -> //for debug
                    failwith(ex.ToString()) 
        }
    asynccall |> Async.StartImmediate
    