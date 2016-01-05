namespace AP.Threading

open System
open System.Collections.Generic
open System.Threading.Tasks

type OneManyMode = Exclusive | Shared

[<Sealed>]
type AsyncOneManyLock() = 
    
    let _lock = new SpinLock(true)
    let _noContentionAccessGranter = Task.FromResult<Object>(null)
    let _qWaitingWriters = new Queue<TaskCompletionSource<Object>>()
    let mutable _waitingReadersSignal = new TaskCompletionSource<Object>()
    let mutable _numWaitingReaders = 0
    let mutable _state = 0

    let lock() =
        let mutable taken = false
        _lock.Enter(&taken)

    let unlock() =
        _lock.Exit()

    let isFree() = _state = 0
    let isOwnedByWriter() = _state = -1
    let isOwnedByReader() = _state > 0
    let addReaders(count: Int32) = _state <- _state + count
    let subtractReader() = _state <- _state - 1
    let makeWriter() = _state <- -1
    let makeFree() = _state <- 0

    member this.WaitAsync(mode: OneManyMode) =
        let mutable accessGranter = _noContentionAccessGranter

        lock()
        match mode with
        | Exclusive -> 
            if isFree() then
                makeWriter()
            else
                let tcs = new TaskCompletionSource<Object>()
                _qWaitingWriters.Enqueue(tcs)
                accessGranter <- tcs.Task
        | Shared ->
            if isFree() || (isOwnedByReader() && _qWaitingWriters.Count = 0) then
                addReaders(1)
            else
                _numWaitingReaders <- _numWaitingReaders + 1
                accessGranter <- _waitingReadersSignal.Task.ContinueWith(
                    fun (t: Task<Object>) -> 
                        t.Result
                )
        unlock()
        accessGranter

    member this.Release() =
        let mutable accessGranted: TaskCompletionSource<Object> option = None

        lock()
        if isOwnedByWriter() then makeFree()
        else subtractReader()

        if isFree() then
            if _qWaitingWriters.Count > 0 then
                makeWriter()
                accessGranted <- Some <| _qWaitingWriters.Dequeue()
            elif _numWaitingReaders > 0 then
                addReaders(_numWaitingReaders)
                _numWaitingReaders <- 0
                accessGranted <- Some <| _waitingReadersSignal
                _waitingReadersSignal <- new TaskCompletionSource<Object>()

        unlock()
        if accessGranted.IsSome then accessGranted.Value.SetResult(null) 