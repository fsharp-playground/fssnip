module ip =    
  open System
  open System.Net

  let intOfIp (s : string) = 
    IPAddress.Parse(s.Trim()).GetAddressBytes() 
    |> Array.rev
    |> fun e -> BitConverter.ToUInt32 (e, 0)
    
  let ipOfInt (d : uint32) = 
    BitConverter.GetBytes d
    |> Array.rev 
    |> fun e -> IPAddress e
    |> string

  let slice (d : string) (iden : string array) = 
    d.Split(iden, StringSplitOptions.None)

  let ipArrayOfIntRange start finish =
    [| for i in start .. finish -> ipOfInt i |]

  let ipsOfRange (d : string) = 
    
    let elem = slice d [|"to"; "-"; "and"|]
    let start,finish = intOfIp elem.[0], intOfIp elem.[1]
    
    ipArrayOfIntRange start finish
    
  (* "192.168.1.1/24" -> ["192.168.1.1 .. 192.168.1.254"] *)
  let ipsOfCidrs (d : string) =
    let elem  = slice d [|"/"|]
    
    let lsn x = (1 <<< 32 - x) - 1 |> (~~~)    |> uint32
    let cidr  = Array.get elem 1   |> int
    let mask  = cidr |> int        |> lsn
    let addr  = elem |> Seq.head   |> intOfIp  |> (&&&) mask
    let start,finish = addr + 1u, addr + ~~~mask - 1u

    if cidr > 30 then [| elem |> Seq.head |]
    else
      ipArrayOfIntRange start finish