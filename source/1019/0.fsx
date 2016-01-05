#r "System.DirectoryServices"
    
open System
open System.IO
open System.DirectoryServices

let cookieName = "cookie.bin"
let resourceDomain = "mydomain.net"
let groupSearchFilter = "(&(objectClass=group)(objectCategory=group)(name=Test Group))"
let cookiePath = String.Format("{0}\{1}", __SOURCE_DIRECTORY__, cookieName)

let getCookie path = 
    match File.Exists(path) with
    | true -> Some(File.ReadAllBytes(path))
    | false -> None

let setCookie path cookie = 
    try
        File.WriteAllBytes(path, cookie)
        Some(cookie)
    with
    | ex -> None

let modifyProxy (user:DirectoryEntry) found notFound =
    user.Properties.["proxyAddresses"]
    |> Seq.cast<string>
    |> Seq.tryFindIndex(fun x -> x.StartsWith("sip:", StringComparison.InvariantCultureIgnoreCase))
    |> function
        | Some(index) ->found user.Properties.["proxyAddresses"] index
        | None -> notFound user.Properties.["proxyAddresses"]
    user.CommitChanges()

let setUserEnabled (user : DirectoryEntry) =
 // Add or Update the proxy address.
    let sip = String.Format("sip:{0}", user.Properties.["mail"].[0].ToString())
    user.Properties.["msRTCSIP-PrimaryUserAddress"].Value <- sip
    modifyProxy user (fun pc i -> pc.[i] <- sip) (fun pc -> pc.Add sip |> ignore)

let setUserDisabled (user : DirectoryEntry) = 
    // Remove the SIP address.
    user.Properties.["msRTCSIP-PrimaryUserAddress"].Clear()
    modifyProxy user (fun pc i -> pc.RemoveAt i) ignore

let update() = 
    let convert (results:ResultPropertyValueCollection) = [for o in results -> new DirectoryEntry(String.Format("LDAP://{0}/{1}", resourceDomain, o))]
    let cookie = getCookie cookiePath
    use de = new DirectoryEntry(String.Format("LDAP://{0}", resourceDomain))
    use searcher = new DirectorySearcher(de, groupSearchFilter)
    let flags = DirectorySynchronizationOptions.ObjectSecurity ||| DirectorySynchronizationOptions.IncrementalValues
    let sync = match cookie.IsNone with
                | true -> new DirectorySynchronization(flags)
                | false -> new DirectorySynchronization(flags, cookie.Value)

    searcher.DirectorySynchronization <- sync
    use res = searcher.FindAll()
    res 
    |> Seq.cast<SearchResult>
    |> Seq.iter(fun x -> 
        (if x.Properties.Contains("member;range=1-1") then convert x.Properties.["member;range=1-1"] else []) |> List.iter setUserEnabled
        (if x.Properties.Contains("member;range=0-0") then convert x.Properties.["member;range=0-0"] else []) |> List.iter setUserDisabled)

    setCookie cookiePath (sync.GetDirectorySynchronizationCookie()) |> ignore 
