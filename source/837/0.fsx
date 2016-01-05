// [snippet:Implementation]
    open System.Collections.Specialized
    open System.Collections.ObjectModel
    
    module ObservableCollection =
      /// Initialize observable collection
      let init n f = ObservableCollection<_>(List.init n f)

      /// Incremental map for observable collections
      let map f (oc:ObservableCollection<'T>) =
        // Create a resulting collection based on current elements
        let res = ObservableCollection<_>(Seq.map f oc)
        // Watch for changes in the source collection
        oc.CollectionChanged.Add(fun change ->
          printfn "%A" change.Action
          match change.Action with
          | NotifyCollectionChangedAction.Add ->
              // Apply 'f' to all new elements and add them to the result
              change.NewItems |> Seq.cast<'T> |> Seq.iteri (fun index item ->
                res.Insert(change.NewStartingIndex + index, f item))
          | NotifyCollectionChangedAction.Move ->
              // Move element in the resulting collection
              res.Move(change.OldStartingIndex, change.NewStartingIndex)
          | NotifyCollectionChangedAction.Remove ->
              // Remove element in the result
              res.RemoveAt(change.OldStartingIndex)
          | NotifyCollectionChangedAction.Replace -> 
              // Replace element with a new one (processed using 'f')
              change.NewItems |> Seq.cast<'T> |> Seq.iteri (fun index item ->
                res.[change.NewStartingIndex + index] <- f item)
          | NotifyCollectionChangedAction.Reset ->
              // Clear everything
              res.Clear()
          | _ -> failwith "Unexpected action!" )
        res
// [/snippet]
    
// [snippet:Sample usage]
    // Create source collection and result using 'map'
    let src = ObservableCollection.init 5 (fun n -> n)
    let res = ObservableCollection.map (fun x -> 
      printfn "processing %d" x; x * 10) src
    
    // Manipulate elements in the source
    src.Move(0, 1)
    src.Remove(0)
    src.Clear()
    src.Add(5)
    src.Insert(0, 3)
    src.[0] <- 1
    
    // Compare the original and the result
    printfn "%A" (Seq.zip src res |> List.ofSeq)
// [/snippet]
