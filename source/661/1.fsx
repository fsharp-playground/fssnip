open Microsoft
open Microsoft.Exchange.WebServices.Data
open System
open System.Net

type PgzExchangeService(url,user,password) =
    let service = new ExchangeService(ExchangeVersion.Exchange2007_SP1)
    do        
       ServicePointManager.ServerCertificateValidationCallback <- ( fun _ _ _ _ -> true )
       service.Url <- new Uri(url)
       service.Credentials <- new WebCredentials(user, password, "pgz")

    member this.Service with get() = service
    member this.InboxItems = this.Service.FindItems(WellKnownFolderName.Inbox, new ItemView(10))
    member this.GetFileAttachments ( item : Item ) =        
           let emailMessage = 
               EmailMessage.Bind( this.Service, 
                                  item.Id, 
                                  new PropertySet(BasePropertySet.IdOnly, ItemSchema.Attachments))
           item, emailMessage.Attachments |> Seq.choose (fun attachment -> match box attachment with  
                                                                           | :? FileAttachment as x -> Some(x) | _ -> None)   
                  
let mailAtdomain = new PgzExchangeService("https://ip/EWS/Exchange.asmx", "user", "password")

let printsave (item : Item ,att : seq<FileAttachment>) =
    if (Seq.length att) > 0 then
        printfn "%A - saving %i attachments" item.Subject (Seq.length att)        
        att |> Seq.iter ( fun attachment -> printfn "%A" attachment.Name 
                                            attachment.Load(@"c:\temp\fsharp\" + attachment.Name ) )   

// filter so we only have items with attachements and ...
let itemsWithAttachments = mailAtdomain.InboxItems                            
                           |> Seq.map mailAtdomain.GetFileAttachments 
                           |> Seq.iter printsave
                           
