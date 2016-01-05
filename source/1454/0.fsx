open System.Threading
open System.Threading.Tasks 

module AsyncHelpers = 
    type Async<'a> with 
        static member public AddCancellation (token:CancellationToken) (a:Async<'a>) : Async<'a option> =
            async {
                let waitCancelTask = new TaskCompletionSource<'a option>()
                use registration = 
                    token.Register(
                        System.Action<obj>(function :? TaskCompletionSource<'a option> as x -> x.TrySetResult(None) |> ignore |_->()), 
                        waitCancelTask)

                let! t_result = 
                    Task.WhenAny(async { let! result = a in return Some(result) } |> Async.StartAsTask, waitCancelTask.Task)
                    |> Async.AwaitTask

                return t_result.Result
            }