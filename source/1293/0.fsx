type [<AbstractClass>] Repository<'t>() =
    abstract Add : 't -> unit
    abstract Update : 't -> unit
    abstract Remove : 't -> unit
    abstract GetAll : unit -> IQueryable<'t>
    abstract Get : ('t -> bool) -> 't
    member r.ItemsToObservableCollection(?seq : seq<'t>) =
        let seq = 
            match seq with
            | Some seq -> seq
            | _ -> r.GetAll() :> seq<_>
        let collection = new ObservableCollection<_>(seq)
        collection.CollectionChanged.Add
            (fun e ->
                match e.Action with
                | NotifyCollectionChangedAction.Add ->
                    e.NewItems |> Seq.cast |> Seq.iter r.Add
                | NotifyCollectionChangedAction.Remove ->
                    e.OldItems |> Seq.cast |> Seq.iter r.Remove
                | _ -> ())
        collection