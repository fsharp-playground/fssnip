open System.DirectoryServices

let modifyProxy (user:DirectoryEntry) found notFound =
    user.Properties.["proxyAddresses"]
    |> Seq.cast<string>
    |> Seq.tryFindIndex(fun x -> x.StartsWith("sip:", StringComparison.InvariantCultureIgnoreCase))
    |> function
        | Some(index) ->found user.Properties.["proxyAddresses"] index
        | None -> notFound user.Properties.["proxyAddresses"]

let proxy (user : DirectoryEntry) =
 // Add or Update the proxy address.
    let sip = String.Format("sip:{0}", user.Properties.["mail"].[0].ToString())
    modifyProxy user (fun pc i -> pc.[i] <- sip) (fun pc -> pc.Add sip |> ignore)

let setUserDisabled (user : DirectoryEntry) = 
    // Remove the SIP address.
    user.Properties.["msRTCSIP-PrimaryUserAddress"].Clear()
    modifyProxy user (fun pc i -> pc.RemoveAt i) (fun _ -> ())