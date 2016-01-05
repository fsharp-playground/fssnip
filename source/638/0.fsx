module async_sleep_fix
    open System
    open System.Collections
    open System.Collections.Generic
    open System.Reactive.Concurrency
    open System.Reactive.Disposables
    open System.Threading
    open System.Threading.Tasks

    type Async with
        static member SleepEx(milliseconds:int) = async{
            if milliseconds > 0 then
                let disp = new SerialDisposable()
                use! ch = Async.OnCancel(fun()->disp.Dispose())
                do! Async.FromContinuations(fun (success, error, cancel) ->
                    let timerSubscription = new SerialDisposable()
                    let CompleteWith = 
                        let completed = ref 0
                        fun cont ->
                            if Interlocked.Exchange(completed, 1) = 0 then
                                timerSubscription.Dispose()
                                try cont() with _->()

                    disp.Disposable <- Disposable.Create(fun()->
                        CompleteWith (fun ()-> cancel(new OperationCanceledException()))
                    )
                    let tmr = new Timer(
                        callback = (fun state -> CompleteWith(success)), 
                        state = null, dueTime = milliseconds, period = Timeout.Infinite
                    )
                    if tmr = null then
                        CompleteWith(fun ()->error(new Exception("failed to create timer")))
                    else
                        timerSubscription.Disposable <- Disposable.Create(fun()->
                            try tmr.Dispose() with _ -> ()
                        )
                )
            else
                return ()
        }
