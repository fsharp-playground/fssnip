open HtmlAgilityPack
open System.Text.RegularExpressions
open System.IO
open System
open System.Text.RegularExpressions
open FSharpx
open FSharpx.Choice

module JsRetriever = 
    let stripHtml text = 
        ["<script\s*"
         "\"?\s*type\s*=\s*\"\s*text/javascript\s*\"\s*"
         "</script>"
         "src\s*=\s*"
         "\""
         ">"
         "</"
         "<"]
        |> List.fold (fun res pattern -> Regex.Replace(res, pattern, "").Trim()) text
    
    let convertToAbsolute parent path = 
        Path.Combine (Path.GetDirectoryName (parent), path) |> Path.GetFullPath
    
    let endsOn ext file = Path.GetExtension(file) = ext

    let getJsFiles (defaultAspxPath: string) = 
        let doc = HtmlDocument()
        doc.Load defaultAspxPath
        doc.DocumentNode.SelectNodes "/html/head/script/@src"
        |> Seq.map (fun x -> x.OuterHtml)
        |> Seq.map (Choice.protect stripHtml >=> Choice.protect (convertToAbsolute defaultAspxPath))
        |> Seq.fold (fun (files, es) -> 
            Choice.choice
                (fun f -> f :: files, es)
                (fun e -> files, e :: es)) ([], [])
        |> fun (files, es) ->
            es |> List.fold (fun acc e -> sprintf "%s, %O" acc e) "" |> printfn "%s"
            files |> Seq.filter (endsOn ".js")
