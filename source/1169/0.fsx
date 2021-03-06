namespace MergeJsFiles

open HtmlAgilityPack 
open System.Text.RegularExpressions
open System.IO
open System

module JsRetriever =
    
    let stripHtml (text:string) = 
        try   
            let mutable target = text
                     
            let regex = [
                "<script\s*", "";            
                "\"?\s*type\s*=\s*\"\s*text/javascript\s*\"\s*", "";                 
                "</script>", "";
                "src\s*=\s*", ""
                "\"", "";
                ">", "";
                "</",""
                "<",""

            ] 
                
            for (pattern, replacement) in regex do
                    target <- Regex.Replace(target,pattern,replacement).Trim()

            target                 
        with
            | ex -> 
                Console.WriteLine ("Error handling " + text + ", " + ex.ToString())
                ""          

    let convertToAbsolute parent path =
        try            
            Path.Combine(Path.GetDirectoryName(parent), path) |> Path.GetFullPath
        with
            | ex -> 
                Console.WriteLine ("Error handling " + path)
                ""
        

    let endsOn ext file = 
        Path.GetExtension(file) = ext
            
    let getJsFiles (defaultAspxPath:string) = 
        let doc = new HtmlDocument()

        doc.Load defaultAspxPath

        doc.DocumentNode.SelectNodes "/html/head/script/@src" 
            |> Seq.map (fun i -> i.OuterHtml) 
            |> Seq.map stripHtml            
            |> Seq.map (convertToAbsolute defaultAspxPath)
            |> Seq.filter (endsOn ".js")
