(* IPv4 conversion snippets FTF*)
module ipv4Conversions = 

  let ip_toint (d : string) = 
      let ele = d.Split '.'
      ((uint32 ele.[0]) <<< 24) 
    + ((uint32 ele.[1]) <<< 16)    
    + ((uint32 ele.[2]) <<< 8)    
    + ((uint32 ele.[3]) )

  let int_toip d =
    let val1 = uint32(d &&& 0xff000000u) >>> 24
    let val2 = uint32(d &&& 0x00ff0000u) >>> 16
    let val3 = uint32(d &&& 0x0000ff00u) >>> 8
    let val4 = uint32(d &&& 0x000000ffu)
    sprintf "%d.%d.%d.%d" val1 val2 val3 val4
    
  (* convert range notation to a list of all ranges *)
  (* possibl replace with pattern matching for a more exhaustive match*)
  let range_toip (d : string) = 
    let range_elem = 
      if d.Contains("-") then d.Split '-'
      else (d.Replace("to"," ")).Split ' '
    
    let elems = range_elem |> Array.filter(fun x -> x.Length > 5)    
    [ip_toint elems.[0] .. ip_toint elems.[1]]
      |> Seq.map int_toip
    
  (* "192.168.1.1/24" -> ["192.168.1.1 .. 192.168.1.254"] *)
  let expand_cidr (d : string) =
    let elem = d.Split '/'
    let cidrBits = int32 elem.[1]
    let mask = uint32 ~~~((1 <<< (32 - cidrBits)) - 1)
    
    (* Network Address *)
    let nwAddress = ip_toint(elem.[0]) &&& mask
    let startAddr = (int_toip(nwAddress + 1u))
    
    (* Broadcast Address *)
    let bcAddress = nwAddress + ~~~mask
    let endinAddr = (int_toip(bcAddress - 1u))
    
    let usableIps = 
      if (cidrBits > 30) 
        then 0u
      else 
        (bcAddress - nwAddress -  1u) 
        
    if usableIps <= 0u 
      then Seq.empty
    else 
      {nwAddress + 1u .. bcAddress - 1u} |> Seq.map int_toip
      
      
(* Examples *)
(*
> ipv4Conversions.ip_toint "192.168.1.1";;
val it : uint32 = 3232235777u

> ipv4Conversions.int_toip 3232235777u;;
val it : string = "192.168.1.1"

> ipv4Conversions.range_toip "192.168.1.10 -192.168.1.20";;
val it : seq<string> =
  seq ["192.168.1.10"; "192.168.1.11"; "192.168.1.12"; "192.168.1.13"; ...]
  
> ipv4Conversions.range_toip "192.168.1.10 to 192.168.1.20";;
val it : seq<string> =
  seq ["192.168.1.10"; "192.168.1.11"; "192.168.1.12"; "192.168.1.13"; ...]
  
> ipv4Conversions.expand_cidr "192.168.1.1/24";;
val it : seq<string> =
  seq ["192.168.1.1"; "192.168.1.2"; "192.168.1.3"; "192.168.1.4"; ...]*)