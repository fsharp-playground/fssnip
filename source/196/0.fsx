module Incrementer = 
    //Interface of our object
    type IncrementerObj = 
        {
            Increment:(int -> unit)
            Decrement:(int -> unit)
            Value:(unit -> int)
        }
    //Internal messages that the object can process
    type private Message = 
                | Increment of int
                | Decrement of int
                | Value of AsyncReplyChannel<int>
    //The constructor for object
    let newIncrementer () = 
        let m = MailboxProcessor<Message>.Start(fun mbox ->
                        let v = ref 0
                        let rec ret () = async {
                                                let! msg = mbox.Receive()
                                                match msg with
                                                | Increment x -> v := !v + x
                                                | Decrement x -> v := !v - x
                                                | Value r -> r.Reply !v
                                                return! ret()
                                            }
                        ret ()
                    )
        {
            Increment = (fun x -> m.Post(Increment x))
            Decrement = (fun x -> m.Post(Decrement x))
            Value = (fun _ -> m.PostAndReply(fun (r:AsyncReplyChannel<int>) -> Value r))
        }


let o = Incrementer.newIncrementer()
o.Increment(10)
o.Increment(100)
o.Decrement(50)
printf "%d" (o.Value())