#if FX_NO_WEBREQUEST_USERAGENT
            | "user-agent" -> if not (req?UserAgent <- value) then try req.Headers.[HeaderEnum.UserAgent] <- value with _ -> ()
#else
            | "user-agent" -> req.UserAgent <- value
#endif