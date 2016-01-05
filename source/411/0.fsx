// [snippet:Async.Choice implementation]
open System.Threading

type Microsoft.FSharp.Control.Async with
  /// Takes several asynchronous workflows and returns 
  /// the result of the first workflow that successfuly completes
  static member Choice(workflows) = 
    Async.FromContinuations(fun (cont, _, _) ->
      let cts = new CancellationTokenSource()
      let completed = ref false
      let lockObj = new obj()
      let synchronized f = lock lockObj f
    
      /// Called when a result is available - the function uses locks
      /// to make sure that it calls the continuation only once
      let completeOnce res =
        let run =
          synchronized(fun () ->
            if completed.Value then false
            else completed := true; true)
        if run then cont res
    
      /// Workflow that will be started for each argument - run the 
      /// operation, cancel pending workflows and then return result
      let runWorkflow workflow = async {
        let! res = workflow
        cts.Cancel()
        completeOnce res }
    
      // Start all workflows using cancellation token
      for work in workflows do
        Async.Start(runWorkflow work, cts.Token) )
// [/snippet]

// [snippet:Sample]
/// Simple function that sleeps for some time 
/// and then returns a specified value
let delayReturn n s = async {
  do! Async.Sleep(n) 
  printfn "returning %s" s
  return s }

// Run two 'delayReturn' workflows in parallel and return
// the result of the first one. Note that this only prints
// 'returning First!' (because the other workflow is cancelled)
Async.Choice [ delayReturn 1000 "First!"; delayReturn 5000 "Second!" ]
|> Async.RunSynchronously
// [/snippet]