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
    
  let findHosts f hosts =
    hosts
    |> Array.map (tryAsyncDnsWithChild f)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Array.filter ((<>) None)
    
  let getFirstEntry a = maybe (Array.get a) 0
  
  let getHostName (h : string) = (Dns.GetHostEntry h).HostName
  let getHostAddr (h : string) =  Dns.GetHostAddresses h |> getFirstEntry |> string
  
  let g f h = [| findHosts f h |> getFirstEntry |> Option.get |]
  
  let ipsOfDomain = g getHostAddr
  let domainOfIp  = g getHostName 