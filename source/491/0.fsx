let callcc (f: ('a -> Async<'b>) -> Async<'a>) : Async<'a> =
  Async.FromContinuations(fun (cont, econt, ccont) ->
    Async.StartWithContinuations(f (fun a -> 
      Async.FromContinuations
        (fun (_, _, _) -> cont a)), cont, econt, ccont)) 

async {
  let cont = ref None
  let! f = callcc (fun f -> async { cont := Some f })
  printfn "!"
  do! Async.Sleep(100) 
  // continuation of async can only be called once,
  // but this call will attempt to call it again
  // and we get an exception...
  return! cont.Value.Value() }
|> Async.Start