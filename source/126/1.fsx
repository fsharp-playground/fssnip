open System
open System.Text.RegularExpressions

let canonicalize (url : string) =
    let domPat = "[^\.]+\.\w{2,3}(\.\w{2})?"
    let url' = Uri.TryCreate(url, UriKind.Absolute)
    let uri =
        match url' with
        | true, str -> Some str
        | _ ->
            let url'' = Uri.TryCreate("http://" + url, UriKind.Absolute)
            match url'' with
            | true, str -> Some str
            | _ -> None
    
    match uri with
    | Some x ->
        let host = x.Host
        let path = x.AbsolutePath
        let host' = Regex(domPat, RegexOptions.RightToLeft).Match(host).Value
        let pattern = "(?i)^https?://((www\.)|([^\.]+\.))" + Regex.Escape(host') + "[^\"]*"
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
let e = canonicalize "google.co.uk"
let f = canonicalize "google.com.au"

// Output:
// val a : string = "http://www.microsoft.com/web"
// val b : string = "http://www.bing.com/"
// val c : string = "http://www.fssnip.net/tags/seq"
// val d : string = "http://fsharp-code.blogspot.com/"
// val e : string = "http://www.google.co.uk/"
// val f : string = "http://www.google.com.au/"