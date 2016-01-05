//reference to the package 
#r "WindowsBase"
//reference to the Xml and Linq 
#r "System.Xml"
#r "System.Xml.Linq"

open System
open System.IO
open System.IO.Packaging
open System.Xml
open System.Xml.Linq

//helpers to create XName with a namespace and namespace
let xname str =
    XName.Get(str, "http://schemas.openxmlformats.org/wordprocessingml/2006/main")

let ns = XNamespace.Get("http://schemas.openxmlformats.org/wordprocessingml/2006/main")

//create the xml from the text (string array)
let createDocument (text:string[]) = 
    //helper to create a paragraph from a string
    let createP (text:string) =
        let el = new XElement(xname "p",
                    new XElement(xname "r",
                        new XElement(xname "t", text)
                        ))
        el
    //the content of the document
    let createContent =
        let content = new XElement(xname "document",
                        new XAttribute(XNamespace.Xmlns + "w", ns),
                            new XElement(xname "body"))
        let r = text |> Array.map createP
        content.Add(r)
        content
    //add the content to the declartion
    let doc =new XDocument(new XDeclaration("1.0", "utf-8", "true"))
    let content = createContent
    doc.Add(content)
    doc

//add the document to package and save
let createFile (xDocument:XDocument) (fileName:string) =
    using (Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite))(fun package ->
        let uri = new Uri("/word/document.xml", UriKind.Relative )
        let partDocumentXML = 
            package.CreatePart(
                                uri,
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml" )
    
        using(new StreamWriter(partDocumentXML.GetStream(FileMode.Create, FileAccess.Write)))(fun stream ->
            xDocument.Save(stream)
        )

        package.CreateRelationship(
                                    uri,
                                    TargetMode.Internal,
                                    "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument",
                                    "rId1") |> ignore
    )


//test
let text = [|"test1"; "test2"; "test3"; "test4"|]

let document = createDocument text

let fileName = @"D:\Tmp\test.docx"

createFile document fileName;;
