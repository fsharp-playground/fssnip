let mutable ck = new CookieContainer()


let GetCookieFromIE ( site: string ) =
  let sw = new SHDocVw.ShellWindowsClass()
  (
    [ for i in sw -> i :?> SHDocVw.InternetExplorer ]
    |> Seq.filter( fun i -> i.FullName.ToLower().Contains("iexplore.exe") )
    |> Seq.filter( fun i -> i.LocationURL.ToLower().Contains( site.ToLower() ) )
    |> Seq.map( fun i -> i.Document :?> mshtml.HTMLDocument )
    |> Seq.map( fun i -> i.cookie )
    |> Seq.head
  ).Split(';')  


let SetCookie site= 
    ( GetCookieFromIE site )
    |> Seq.iter( fun i -> ck.SetCookies(new Uri( site ), i) )


SetCookie "http://site.site.site"