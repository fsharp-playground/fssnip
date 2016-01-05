open System

type CacheMessage<'a,'b> =
| Get of 'a * AsyncReplyChannel<'b option>
| Set of 'a * 'b

let cache timeout =
   let expiry = TimeSpan.FromMilliseconds (float timeout)
   let exp =
      Map.filter (fun _ (_,dt) -> DateTime.Now-dt >= expiry)
   let newValue k v = Map.add k (v, DateTime.Now)
   MailboxProcessor.Start(fun inbox ->
      let rec loop map =
         async {
            let! msg = inbox.TryReceive timeout
            match msg with
            | Some (Get (key, channel)) ->
               match map |> Map.tryFind key with
               | Some (v,dt) when DateTime.Now-dt < expiry ->
                  channel.Reply (Some v)
                  return! loop map
               | _ ->
                  channel.Reply None
                  return! loop (Map.remove key map)
            | Some (Set (key, value)) ->
               return! loop (newValue key value map)
            | None ->
               return! loop (exp map)
         }
      loop Map.empty
   )

// sample usage:
// let c = cache 1500
// let get k = c.PostAndReply (fun channel -> Get (k, channel))
// let set k v = c.Post (Set (k, v))
// set 5 "test"
// printfn "5 => %A" (get 5)
// ... wait for 1.5 seconds, expiry will kick in ...
// printfn "5 => %A" (get 5)