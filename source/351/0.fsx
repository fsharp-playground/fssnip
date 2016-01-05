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

let spreadsheetML = @"http://schemas.openxmlformats.org/spreadsheetml/2006/main"
let relationSchema = @"http://schemas.openxmlformats.org/officeDocument/2006/relationships"
let workbookContentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"
let worksheetContentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"

let xnameEmpty str =
    XName.Get(str)
    
let xname str =
    XName.Get(str, spreadsheetML)

let ns = XNamespace.Get(spreadsheetML)
let nsRelation = XNamespace.Get(relationSchema)

let createPackagePart uriString contentType (xDocument:XDocument) (package:Package) =
    let uri  = new Uri(uriString, UriKind.Relative)
    let part = package.CreatePart(uri, contentType)
    using (new StreamWriter(part.GetStream(FileMode.Create, FileAccess.Write)))(fun stream ->
            xDocument.Save(stream)
        )
    (uri, part)

let createDocument (content:XElement) =
    let doc =new XDocument(new XDeclaration("1.0", "utf-8", "true"))
    content |> doc.Add
    doc

let createWorkBook sheetName=
    let content = new XElement(xname "workbook",
                    new XAttribute(XNamespace.Xmlns + "x", ns),
                    new XElement(xname "sheets",
                        new XElement(xname "sheet",
                            new XAttribute(xnameEmpty "name", sheetName),
                            new XAttribute(xnameEmpty "sheetId", "1"),
                            new XAttribute(XName.Get("id", relationSchema), "rId1"),
                            new XAttribute(XNamespace.Xmlns + "r", nsRelation))))
    content |> createDocument 

let createSheet (sheetData:XElement)=
    let content = new XElement(xname "worksheet",
                    new XAttribute(XNamespace.Xmlns + "x", ns),
                    new XElement(sheetData))
    content |> createDocument 

//add the document to package and save
let createFile (sheetData:XElement) (fileName:string)  =
    using (Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite))(fun package ->
        
        let (uriStartPart,partWorkbook) = createPackagePart "/xl/workbook.xml" workbookContentType (createWorkBook "test") package
        let (uriWorksheet, _ ) = createPackagePart "/xl/worksheets/sheet1.xml" worksheetContentType (createSheet sheetData)package

        package.CreateRelationship(uriStartPart, TargetMode.Internal, relationSchema+"/officeDocument", "rId1") |> ignore
        partWorkbook.CreateRelationship(uriWorksheet, TargetMode.Internal, relationSchema + "/worksheet", "rId1") |> ignore

    )


let sheetData = new XElement(xname "sheetData")
let fileName = @"D:\Tmp\test.xlsx"
createFile sheetData fileName;;

let sheetData2 = new XElement(xname "sheetData",
                                new XElement(xname "row",
                                    new XAttribute(xnameEmpty "r", "1"),
                                    new XElement(xname "c",
                                        new XAttribute(xnameEmpty "r", "A1"),
                                        new XAttribute(xnameEmpty "t", "inlineStr"),
                                            new XElement(xname "is",
                                                new XElement(xname "t", "test"))),
                                    new XElement(xname "c",
                                        new XAttribute(xnameEmpty "r", "B1"),
                                        new XAttribute(xnameEmpty "t", "n"),
                                            new XElement(xname "v", 123
                                                ))
                                ))

let fileName2 = @"D:\Tmp\test2.xlsx"
createFile sheetData2 fileName2;;
