module TwitterFeed

open System
open System.Configuration
open System.IO
open System.Xml
open System.Linq
open System.Xml.Linq
open System.Xml.XPath
open System.Web
open System.Net
open System.Collections.Generic
open System.Text
open System.Security.Cryptography

open System.Runtime.Serialization.Json
   
// create the signature for a twitter web request
let createSignature (reqMethod : string) (url : string) (parameters : (string*string) list) (oAuthParameters : (string*string) list) (oAuthConsumerSecret : string) (oAuthTokenSecret : string) = 
    let sigBaseString = System.Text.StringBuilder()
    sigBaseString.Append(reqMethod) |> ignore
    sigBaseString.Append("&") |> ignore
    sigBaseString.Append(Uri.EscapeDataString(url)) |> ignore
    sigBaseString.Append("&") |> ignore
    List.append parameters oAuthParameters
    |> List.sort
    |> List.iter (fun (key, value) -> sigBaseString.Append(Uri.EscapeDataString(System.String.Format("{0}={1}&", key, value))) |> ignore)
    let signatureBaseString = sigBaseString.ToString().Substring(0, sigBaseString.Length - 3)
    let signatureKey = Uri.EscapeDataString(oAuthConsumerSecret) + "&" + Uri.EscapeDataString(oAuthTokenSecret)
    let hmacsha1 = new HMACSHA1(ASCIIEncoding().GetBytes(signatureKey))
    let signatureString = Convert.ToBase64String(hmacsha1.ComputeHash(ASCIIEncoding().GetBytes(signatureBaseString)))
    signatureString

// create the authorization header for a twitter web request
let createAuthorizationHeaderParameter (signature : string) (oAuthParameters : (string*string) list) = 
    let authorizationHeaderParams = System.Text.StringBuilder()
    authorizationHeaderParams.Append("OAuth ") |> ignore
    oAuthParameters @ [("oauth_signature", signature)]
    |> List.sort
    |> List.iteri (fun i (key, value) -> 
        if i > 0 then authorizationHeaderParams.Append(", ") |> ignore        
        authorizationHeaderParams.Append(key) |> ignore
        authorizationHeaderParams.Append("=\"") |> ignore
        authorizationHeaderParams.Append(Uri.EscapeDataString(value)) |> ignore
        authorizationHeaderParams.Append("\"") |> ignore)
    authorizationHeaderParams.ToString()

// perform a twitter query - be careful to not exceed the twitter limits - 1 per minute should be enough
let query() = 
    let baseUrl = "https://api.twitter.com/1.1/statuses/home_timeline.json"
    let parameters = [("screen_name", ScreenName); ("count", "20")]
    let oAuthParameters = 
        [("oauth_consumer_key", OAuthConsumerKey);
         ("oauth_nonce", System.Guid.NewGuid().ToString());
         ("oauth_signature_method", "HMAC-SHA1");
         ("oauth_timestamp", System.Math.Round(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds, 0).ToString());
         ("oauth_token", OAuthToken);
         ("oauth_version", "1.0");]
         
    
    let url = System.Text.StringBuilder()
    url.Append(baseUrl) |> ignore    
    parameters |> List.iteri (fun i (key, value) -> 
        if i = 0 then url.Append("?") |> ignore
        else url.Append("&") |> ignore
        url.Append(key) |> ignore
        url.Append("=") |> ignore
        url.Append(value) |> ignore)
    let signature = createSignature "GET" baseUrl parameters oAuthParameters OAuthConsumerSecret OAuthTokenSecret
    let authorizationHeaderParams = createAuthorizationHeaderParameter signature oAuthParameters
    let twitterQuery = WebRequest.Create(url.ToString());
    twitterQuery.Method <- "GET"
    twitterQuery.Headers.Add("Authorization: " + authorizationHeaderParams)

    let response = twitterQuery.GetResponse();

    let responseText = (new System.IO.StreamReader(response.GetResponseStream())).ReadToEnd()

    responseText
