// Run as console app or fsx
open Suave.Http
open Suave.Types
open Suave.Http.Successful
open Suave.Http.Writers
open Suave.Http.Applicatives
open Suave.Web

let allow_cors : WebPart =
    choose [
        OPTIONS >>= 
            fun context -> 
                set_header  "Access-Control-Allow-Origin" "*" context 
                |> OK "CORS approved"
    ]

let webSite =
    choose [
        allow_cors
        GET >>= OK "URLs are for wimps. GETting something? This is what you get."
    ]

web_server default_config webSite

(*
=== A fiddler scratchpad to test

OPTIONS http://localhost:8083/ HTTP/1.1
User-Agent: Fiddler
Origin: http://www.example-social-network.com
Host: localhost:8083
*)