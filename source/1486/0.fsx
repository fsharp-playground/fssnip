module List =

    /// <summary>
    ///     fold by key combinator
    /// </summary>
    /// <param name="proj">Key projection function.</param>
    /// <param name="folder">Folding function.</param>
    /// <param name="init">Initial state function.</param>
    /// <param name="ts">Input list.</param>
    let foldBy (proj : 'T -> 'Key) (folder : 'State -> 'T -> 'State) (init : unit -> 'State) (ts : 'T list) =
        let dict = new System.Collections.Generic.Dictionary<'Key, 'State ref>()

        let rec aux = function
            | [] -> ()
            | t :: ts ->
                let k = proj t
                let ok, cached = dict.TryGetValue k
                let state = 
                    if ok then cached 
                    else 
                        let state = ref <| init ()
                        dict.[k] <- state
                        state

                state := folder state.Value t
                aux ts

        aux ts

        dict |> Seq.map (fun kv -> kv.Key, kv.Value.Value) |> Seq.toList


    // example: implementing other known combinators in terms of foldBy

    let groupBy (proj : 'T -> 'Key) (ts : 'T list) =
        foldBy proj (fun ts t -> t :: ts) (fun () -> []) ts

    let countBy (proj : 'T -> 'Key) (ts : 'T list) =
        foldBy proj (fun n _ -> n + 1) (fun _ -> 0) ts