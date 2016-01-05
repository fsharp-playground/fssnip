open System
open System.Text.RegularExpressions

let canonicalize (url : string) =
    let domPat = "([^\.]+\.)?([^\.]+\.[^/]+)"
    let url' = Uri.TryCreate(url, UriKind.Absolute)
    let uri =
        match url' with
        | true, str -> Some str
        | _ -> let url'' = Uri.TryCreate("http://" + url, UriKind.Absolute)
               match url'' with
               | true, str -> Some str
               | _ -> None
    match uri with
    | Some x -> let host = x.Host
                let path = x.AbsolutePath
                let host' = Regex(domPat).Match(host).Groups.[2].Value
                let pattern = "(?i)^(http|https)://((www\.)|([^\.]+\.))" + Regex.Escape(host') + "[^\"]*"
                let m = Regex(pattern).IsMatch(string x)
                match m with
                | true -> "http://" + host + path
                | false -> "http://www." + host + path
    | None -> ""

// Example:
let a = canonicalize "microsoft.com/web"
let b = canonicalize "www.bing.com"
let c = canonicalize "http://fssnip.net/tags/seq"
let d = canonicalize "fsharp-code.blogspot.com"

// Output:
// val a : string = "http://www.microsoft.com/web"
// val b : string = "http://www.bing.com/"
// val c : string = "http://www.fssnip.net/tags/seq"
// val d : string = "http://fsharp-code.blogspot.com/"