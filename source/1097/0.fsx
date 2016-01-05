let myUser func =
    func "domain\\myUser"
    
let upload user file =
    printf "%s uploaded the file %s" user file
    
let files = ["lorem.docx"; "ipsum.pdf"; "dolor.xls"; "amet.pptx"]

let bulkupload user (fileCollection : List<string>) =
    for file in fileCollection do
        upload user file

let firstFileIn (list:List<string>) =
    list.Head

myUser upload (firstFileIn files)
myUser bulkupload files