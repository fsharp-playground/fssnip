// Learn more about F# at http://fsharp.net

open Microsoft
open Microsoft.Exchange.WebServices.Data
open System
open System.Net

ServicePointManager.ServerCertificateValidationCallback <- ( fun _ _ _ _ -> true )

let exchangeService =    
    let service = new ExchangeService(ExchangeVersion.Exchange2007_SP1)    
    service.Url <- new Uri("https://ip_exchange/EWS/Exchange.asmx")
    service.Credentials <- new WebCredentials("user", "password", "domain")            
    service

let readMailBox =         
    let items = exchangeService.FindItems(WellKnownFolderName.Inbox, new ItemView(10))    
    let itemsEnumerator = items.GetEnumerator()    
    seq { while itemsEnumerator.MoveNext() do yield itemsEnumerator.Current }

let processItem messageID =
    let emailMessage = EmailMessage.Bind( exchangeService, messageID, new PropertySet(BasePropertySet.IdOnly, ItemSchema.Attachments)) 
    let attachmentEnumerator = emailMessage.Attachments.GetEnumerator()
    let attachments = seq { while attachmentEnumerator.MoveNext() do
                                 yield attachmentEnumerator.Current }
    let fileAttachments = attachments |> Seq.filter ( fun attachment -> match attachment with
                                                                        | :? FileAttachment -> true
                                                                        |_ -> false )
                                      |> Seq.cast
    printfn "saving"
    for fileAttachment : FileAttachment in fileAttachments do         
        let fileName = String.concat "" ["c:\\temp\\"; fileAttachment.Name ] 
        fileAttachment.Load(fileName)
                
let main = 
    // read all items from mailbox and ...    
    let items = readMailBox       
    // filter so we only have items with attachements and ...
    let itemsWithAttachments = items |> Seq.filter ( fun item -> item.HasAttachments ) 
    // process every item based in ID
    for item in itemsWithAttachments do processItem item.Id                      

main