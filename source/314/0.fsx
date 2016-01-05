//reference to the Open Office SDK
#r @"C:\Program Files (x86)\Open XML SDK\V2.0\lib\DocumentFormat.OpenXml.dll"
//reference to the package 
#r "WindowsBase"

open DocumentFormat.OpenXml
open DocumentFormat.OpenXml.Packaging
open DocumentFormat.OpenXml.Spreadsheet

let createSpreadsheet (filepath:string) (sheetName:string) =
    // Create a spreadsheet document by supplying the filepath.
    // By default, AutoSave = true, Editable = true, and Type = xlsx.
   
    using (SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook)) (fun spreadsheetDocument ->

    // Add a WorkbookPart to the document.
    let workbookpart = spreadsheetDocument.AddWorkbookPart()
    workbookpart.Workbook <- new Workbook()

    // Add a WorksheetPart to the WorkbookPart.
    // http://stackoverflow.com/questions/5702939/unable-to-append-a-sheet-using-openxml-with-f-fsharp
    let worksheetPart = workbookpart.AddNewPart<WorksheetPart>()
    worksheetPart.Worksheet <- new Worksheet(new SheetData():> OpenXmlElement)

    // Add Sheets to the Workbook.
    let sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets())

    // Append a new worksheet and associate it with the workbook.
    let sheet = new Sheet()
    sheet.Id <-  StringValue(spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart))
    sheet.SheetId <-  UInt32Value(1u) 
    sheet.Name <-  StringValue(sheetName)
    sheets.Append([sheet :> OpenXmlElement])
    )

let result = createSpreadsheet @"D:\Tmp\test1.xlsx" "test";;

