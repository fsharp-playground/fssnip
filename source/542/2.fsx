(* IPv4 conversion snippets FTF*)
open System.Net
open System.Text.RegularExpressions

let args = System.Environment.GetCommandLineArgs()

module ipv4Convert = 

  let ip_toint (d : string) = 
    d.Split '.' 
    |> Seq.map uint32 
    |> Seq.map2 (fun bits ele -> ele <<< bits) [ 24; 16; 8; 0 ] 
    |> Seq.sum

  let int_toip d =
    let vals = 
      [| 24; 16; 8; 0 |] 
      |> Array.map(fun bits -> (d &&& (0xffu <<< bits)) >>> bits)
    
    sprintf "%d.%d.%d.%d" vals.[0] vals.[1] vals.[2] vals.[3]

  (* convert range notation to a list of all ranges *)
  let range_toip (d : string) = 
  
    let elems = 
      d.Split([|"-";"to"|], System.StringSplitOptions.None) 
      |> Array.map (fun s -> s.Trim()) 
      
    { ip_toint elems.[0] .. ip_toint elems.[1] } 
    |> Seq.map int_toip
    
  (* "192.168.1.1/24" -> ["192.168.1.1 .. 192.168.1.254"] *)
  let expand_cidr (d : string) =
    let elem = d.Split '/'
    let cidrBits = int32 elem.[1]
    let mask = uint32 ~~~((1 <<< (32 - cidrBits)) - 1)
    let nwAddress = ip_toint(elem.[0]) &&& mask
    
    if cidrBits > 30 then
      Seq.empty
    else
      {nwAddress + 1u .. (nAddr + ~~~mask) - 1u} 
      |> Seq.map int_toip
    
      
      
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