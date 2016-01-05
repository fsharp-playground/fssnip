 async {
     let req = FtpWebRequest.Create(ftpUrl) :?> FtpWebRequest
     req.Method <- WebRequestMethods.Ftp.DownloadFile
     let! response = req.AsyncGetResponse()
     use stream = response.GetResponseStream()
     return streamMap(stream)
 } |> (fun x -> Async.RunSynchronously(x, timeout.TotalMilliseconds |> int))
