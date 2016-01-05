type Message<'a> =
  | Enqueue of 'a
  | Dequeue of (seq<'a> -> unit)

while true do
  do
    let n = 1000000
    let agent =
      MailboxProcessor.Start(fun inbox ->
        let msgs = Array.zeroCreate n
        let i = ref 0
        let rec loop() =
          async { let! msg = inbox.Receive()
                  match msg with
                  | Enqueue x ->
                      msgs.[!i] <- x
                      incr i
                      return! loop()
                  | Dequeue reply ->
                      reply msgs }
        loop())
    let timer = System.Diagnostics.Stopwatch.StartNew()
    for i=1 to n do
      agent.Post(Enqueue i)
    agent.PostAndReply(fun reply -> Dequeue reply.Reply)
    |> ignore
    printfn "%f msgs/s" (float n / timer.Elapsed.TotalSeconds)

  do
    let n = 1000000
    let queue = ResizeArray()
    use barrier = new System.Threading.Barrier(2)
    System.Threading.Thread(fun () ->
      let msgs = Array.zeroCreate n
      let i = ref 0
      let msg = ref Unchecked.defaultof<_>
      let rec loop() =
        let xs = lock queue (fun () -> let xs = queue.ToArray() in queue.Clear(); xs)
        let rec iter j =
          if j = xs.Length then loop() else
            match xs.[j] with
            | Enqueue x ->
                msgs.[!i] <- x
                incr i
                iter (j+1)
            | Dequeue reply ->
                reply msgs
                barrier.SignalAndWait()
        iter 0
      loop()).Start()
    let timer = System.Diagnostics.Stopwatch.StartNew()
    for i=1 to n do
      lock queue (fun () -> queue.Add(Enqueue i))
    let msgs = ref Seq.empty
    lock queue (fun () -> queue.Add(Dequeue(fun xs -> msgs := xs)))
    barrier.SignalAndWait()
    let t = timer.Elapsed.TotalSeconds
    printfn "%f msgs/s" (float(Seq.length !msgs) / t)
