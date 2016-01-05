module TwitterAuth
open System
open System.Text
open System.Net

let percentEncode (sb:StringBuilder) (ch:char) =
    if   '0' <= ch && ch <= '9' then sb.Append(ch)
    elif 'a' <= ch && ch <= 'z' then sb.Append(ch)
    elif 'A' <= ch && ch <= 'Z' then sb.Append(ch)
    else
        match ch with 
        | '-' | '.' | '_'  | '~' -> sb.Append(ch)
        | _ -> 
            sb.Append('%') |> ignore
            sb.AppendFormat("{0:X2}",int(ch))

let (+=) (sb:StringBuilder) (a:string) = sb.Append(a)
let (!*) (s:string) = 
    let sb = StringBuilder()
    for i in 0..s.Length-1 do
        let ch = s.[i]
        percentEncode sb ch |> ignore
    sb.ToString()

let rnd = System.Random()
let nonce() =
    let bytes = Array.create 16 0uy
    rnd.NextBytes(bytes)
    BitConverter.ToString(bytes).Replace("-","")

let epoch = DateTime(1970,1,1)
let timeStamp() =
    let dt = (DateTime.UtcNow - epoch).TotalSeconds |> int
    dt.ToString()

let toUpper (s:string) = s.ToUpper()

let signingKey (consumerSecret:string) tokenSecret = 
    consumerSecret + "&" + tokenSecret
    |> Encoding.ASCII.GetBytes

let parameterString parameters =
    let amp = "&"
    let eq  = "="
    let sb = (new StringBuilder(), parameters) ||> Map.fold (fun sb k v -> sb += k += eq += v += amp)
    sb.Remove(sb.Length-amp.Length,amp.Length).ToString()

let sigBase httpMethod baseUrl parmDataStr = 
    (httpMethod |> toUpper) + "&" + !* baseUrl + "&" + !* parmDataStr

let hmacSha1Sig signKey dataString =
    let data = (dataString:string) |> Encoding.ASCII.GetBytes
    use alg = new System.Security.Cryptography.HMACSHA1(signKey)
    alg.ComputeHash(data) |> Convert.ToBase64String

let buildAuthHdr headerParameters =
    assert (headerParameters |> Map.toSeq |> Seq.length = 7)
    let sb = StringBuilder()
    sb.Append("OAuth ") |> ignore
    let sb = (sb,headerParameters) ||> Map.fold (fun sb k v -> sb += k += "=" += "\"" += v += "\"" += ", ")
    sb.Remove(sb.Length - 2, 2) |> ignore
    let header = sb.ToString()
    header

///Generate the Authorization header string based on the request parameters and keys and tokens
//The process is defined here: https://dev.twitter.com/docs/auth/authorizing-request
let authHeader 
    signKey
    consumerKey
    token
    httpMethod
    baseUrl
    reqParams =
    let secParams =
        [
            "oauth_consumer_key",           !*consumerKey
            "oauth_signature_method" ,      "HMAC-SHA1"
            "oauth_timestamp",              timeStamp()
            "oauth_token",                  !*token
            "oauth_version",                "1.0"
            "oauth_nonce",                  nonce()
        ] |> Map.ofList   
    let parameters = (secParams,reqParams) ||> Map.fold(fun acc k v -> acc|>Map.add !*k !*v)
    let parmStr = parameters |> parameterString
    let data2Sign = sigBase httpMethod baseUrl parmStr
    let sigData = hmacSha1Sig signKey data2Sign
    let headerParameters = 
        parameters 
        |> Map.filter (fun k _ -> k.StartsWith("oauth_") || k="realm")
        |> Map.add "oauth_signature" !*sigData
    buildAuthHdr headerParameters

//test parameters are from here
//https://dev.twitter.com/docs/auth/creating-signature
let testSig() =
    let parameters = 
        [
            "status",                       "Hello Ladies + Gentlemen, a signed OAuth request!"
            "include_entities",             "true"
            "oauth_consumer_key",           "xvz1evFS4wEEPTGEFPHBog"
            "oauth_nonce",                  "88643f5b760c4780814a7101ef796851"
            "oauth_signature_method" ,      "HMAC-SHA1"
            "oauth_timestamp",              "1318622958"
            "oauth_token",                  "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb"
            "oauth_version",                "1.0"
        ]
        |> List.map (fun (k,v) -> !*k,!*v)
        |> Map.ofList
    let consumerSecret = "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw"
    let tokenSecret = "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE"
    let signKey = signingKey consumerSecret tokenSecret
    let httpMethod = "post"
    let baseUrl = "https://api.twitter.com/1/statuses/update.json"
    let parmStr = parameters |> parameterString
    let data = sigBase httpMethod baseUrl parmStr
    let mySig = hmacSha1Sig signKey data
    mySig

let toParmString parms =
    let sb = (StringBuilder(),parms) ||> Map.fold (fun sb k v -> sb += !*k += "=" += !*v += "&")
    sb.Remove(sb.Length-1,1).ToString()

let download (url:string) hdr =
    use wc = new WebClient()
    wc.Headers.Add(HttpRequestHeader.Authorization,hdr)
    wc.DownloadString(url)

//substitute where you see '****' with appropriate values, to test your keys and tokens
let myExample() =
    let consumerKey = "<****my consumer key"
    let consumerSecret = "<****my consumer secret>"
    let token = "<****my access token>"
    let tokenSecret = "<****my token secret>"
    let signKey = signingKey consumerSecret tokenSecret
    let baseUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json"
    let httpMethod = "get"
    let requestQuery = ["user_id", "<****my userid>"] |> Map.ofList
    let authHeader = authHeader signKey consumerKey token httpMethod baseUrl requestQuery
    let url = baseUrl + "?" + (requestQuery |> toParmString)
    download url authHeader
