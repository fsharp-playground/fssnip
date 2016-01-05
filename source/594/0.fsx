open System
open System.Net

let getBetterCredentials=
    new NetworkCredential("user", "password")

// [snippet:Active pattern]
let (|HttpException|_|) (ex:Exception) =
    match ex with
    | :? WebException as webException ->
        match webException.Response with
        | :? HttpWebResponse as response ->
            Some response.StatusCode
        | _ -> None
    | _ -> None
// [/snippet]

// [snippet:Example usage]
let fetch url = 
    let webClient = new WebClient()
    let rec get (url:string) =
        try
            Some (webClient.DownloadString(url))
        with
            | HttpException HttpStatusCode.Unauthorized ->
                webClient.Credentials <- getBetterCredentials
                get url
            | HttpException statusCode ->
                printfn "Failed, server said: %A" statusCode
                None
    get url
//[/snippet]