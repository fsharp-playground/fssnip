static member StartChild (computation:Async<'T>,?millisecondsTimeout) =
    async { 
        let resultCell = new ResultCell<_>()
        let! ct = getCancellationToken()
        
        // ** the below comment is wrong, adding use removes the memory leak **

        let innerCTS = new CancellationTokenSource() // innerCTS does not require disposal
        let ctsRef = ref innerCTS
        let _reg = ct.Register(
                                (fun _ -> 
                                    match !ctsRef with
                                    |   null -> ()
                                    |   otherwise -> otherwise.Cancel()), 
                                null)
        do queueAsync 
                innerCTS.Token
                                              
                (fun res -> ctsRef := null; resultCell.RegisterResult (Ok res, reuseThread=true))   
                (fun err -> ctsRef := null; resultCell.RegisterResult (Error err,reuseThread=true))   
                (fun err -> ctsRef := null; resultCell.RegisterResult (Canceled err,reuseThread=true))    
                       
                computation
                |> unfake