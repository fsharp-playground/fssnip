module Demo =
    open System.Net.Mail
        let SendTest() =
            let msg = new MailMessage("me@mydomain.com", @"you@yourdomain.com", @"A test", 
                                        "The lesser world was daubed\nBy a colorist of modest skill.\n\
                                        A master limned you in the finest inks\nAnd with a fresh-cut quill.\n")
            msg.Bcc.Add(@"them@theirdomain.com")

            let client = new SmtpClient(@"smtpmail.xx.yyyy.com")

            let pretend = false
            if pretend then
                let pickupDir =  @"d:\temp\pretendmail"
                System.IO.Directory.CreateDirectory(pickupDir) |> ignore
                client.DeliveryMethod <- SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation <- pickupDir
            else
                client.DeliveryMethod <- SmtpDeliveryMethod.Network

            client.Send(msg)