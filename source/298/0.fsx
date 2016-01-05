let myTimeStamp = 
    let zone = System.TimeZone.CurrentTimeZone.GetUtcOffset System.DateTime.Now
    let prefix = match (zone<System.TimeSpan.Zero) with | true -> "-" | _ -> "+"
    System.DateTime.UtcNow.ToString("yyyyMMddHHmmssffff") + prefix + zone.ToString("hhss");     
