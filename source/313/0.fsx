//reference to the Open Office SDK
#r @"C:\Program Files (x86)\Open XML SDK\V2.0\lib\DocumentFormat.OpenXml.dll"
//reference to the package 
#r "WindowsBase"

open DocumentFormat.OpenXml;
open DocumentFormat.OpenXml.Wordprocessing
open DocumentFormat.OpenXml.Packaging

let testString = "This is a test"
let printXml text =
    printfn "xml: %s" text

let createBody (text:string) =
    let text = new Text(text)
    let run = new Run()
    run.AppendChild(text) |> ignore
    let para = new Paragraph()
    para.AppendChild(run)|> ignore
    let body = new Body()
    body.AppendChild(para)|> ignore
    body

printXml (createBody testString).InnerXml

let createDocument (text:string) =
   let body = createBody text
   let doc = new Document()
   doc.AppendChild(body) |> ignore
   doc

printXml (createDocument testString).InnerXml

let createWordprocessingDocument (filepath:string) text=
    using (WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document)) (fun doc ->
    let mainPart = doc.AddMainDocumentPart();
    mainPart.Document <- createDocument text 
    )

let result3 = createWordprocessingDocument @"D:\Tmp\test1.docx" testString