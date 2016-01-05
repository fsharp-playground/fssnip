open System.Threading
let cts=new CancellationTokenSource()
let rec loop i=
  async{do! Async.Sleep 1000
        printfn"loop----%d" i
//        if i>5 then failwith "i>5"
        if i>6 then ()
        else return! loop<|i+1
  }
Async.StartWithContinuations(
        loop 0,
        (fun cont->printfn "cont-%A" cont),
        (fun exn->printfn"exception-%s"<|exn.ToString()),
        (fun exn->printfn"cancell-%s"<|exn.ToString()),
        cts.Token)
cts.Cancel()
