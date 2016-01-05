//#load "AppId.fsx"
#r "System.Web.Extensions.dll"
#r "System.Runtime.Serialization.dll"
;;
open System
open System.Net
open System.Text
open System.IO
open System.Web.Script.Serialization
open System.Runtime.Serialization
open System.Globalization
;;
[<DataContract>]
type TweetUser = {
    [<field:DataMember(Name="followers_count")>] Followers:int
    [<field:DataMember(Name="screen_name")>] Name:string
    [<field:DataMember(Name="id_str")>] Id:int
    [<field:DataMember(Name="location")>] Location:string}

[<DataContract>]
type Tweet = {
     [<field:DataMember(Name="text")>] Text:string
     [<field:DataMember(Name="retweeted")>] IsRetweeted:bool
     [<field:DataMember(Name="created_at")>] DateStr:string
     [<field:DataMember(Name="user")>] User:TweetUser
     [<field:DataMember(Name="geo")>] Geo:string}

let dser = Json.DataContractJsonSerializer(typeof<Tweet>)
let toStream (b:byte array) = new MemoryStream(b)
let twitterDateFormat = "ddd MMM dd HH:mm:ss zzz yyyy"; //"Fri Oct 07 18:41:13 +0000 2011"
let toDateTime s = DateTime.ParseExact(s, twitterDateFormat, CultureInfo.InvariantCulture)
let mutable connections = []


let tweets tracking =
    let template = sprintf "https://stream.twitter.com/1/statuses/filter.json?track=%s"
    let http = WebRequest.Create(template tracking) :?> HttpWebRequest
//    http.Credentials <- NetworkCredential(AppId.twitterId, AppId.twitterPwd)
    http.Credentials <- NetworkCredential("<twitter id>", "<twitter password>")
    http.Timeout <- -1
    let resp = http.GetResponse()
    let str = resp.GetResponseStream()
    connections <- http::connections
    let readStream = new StreamReader(str, Encoding.UTF8)
    readStream |> Seq.unfold(fun rs ->
            try
                let line = rs.ReadLine() 
                if line.StartsWith("{\"text")  then 
                    let tweet =
                        try
                            line
                            |> Encoding.UTF8.GetBytes
                            |> toStream
                            |> dser.ReadObject :?> Tweet
                            |> Some
                        with
                        | ex -> 
                            printfn "Error %A" ex.Message
                            None
                    printfn "%A" tweet.Value
                    Some(tweet, rs)
                else
                    Some(None, rs)
            with
            | ex -> 
                printfn "%s" ex.Message
                None)
    |> Seq.choose (fun t -> t)

let stopAll() =
    connections |> Seq.iter (fun s -> printfn "closing..."; s.Abort())
;;
let tweetsPerMinute tracking =
    tweets tracking
    |> Seq.map (fun t -> toDateTime t.DateStr)
    |> Seq.windowed 3
    |> Seq.map (fun ts -> (Seq.max ts) - (Seq.min ts), ts.Length)
    |> Seq.map (fun (interval,count) -> (float)count / interval.TotalMinutes)

(* Usage:
async {tweets("soccer") |> Seq.iter(fun t -> printfn "%A" t)} |> Async.Start
async {tweetsPerMinute "obama" |> Seq.iter (fun m -> printfn "[%0.0f per/min]" m) } |> Async.Start
stopAll()
*)