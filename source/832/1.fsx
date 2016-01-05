let maybe f x = try Some (f x) with _ -> None

module ns = 
  
  open System
  open System.Net
  let child f = Async.StartChild(f, 25200) 

  let maybeAsync f x = async { return (maybe f x) }

  let childAsync f = 
    async { try let! x = f in return! x with _ -> return None }

  let tryAsyncDnsWithChild f = 
    maybeAsync f >> child >> childAsync
    
  (* kinda redundant (parallelism wise) because we do 1 host at a time, look into fixing later*)
  let findHosts f hosts =
    hosts
    |> Array.map (tryAsyncDnsWithChild f)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Array.filter ((<>) None)
    |> Array.map Option.get
     
  let getFirstEntry a = maybe (Array.get a) 0 |> Option.get
  
  let getHostEntry (h : string) = Dns.GetHostEntry h
  
  let getHostName h = (getHostEntry h).HostName
  let getHostAddr h = (getHostEntry h).AddressList |> getFirstEntry |> string
  
  let g f h = [| findHosts f h |> getFirstEntry |]
  
  let ipsOfDomain = g getHostAddr
  let domainOfIp  = g getHostName 