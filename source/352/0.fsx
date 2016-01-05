type AutoCompleteFunction = AutoCompleteFunction of Lazy<string[]> * (string -> AutoCompleteFunction)
with
    member x.Call s =
        let (AutoCompleteFunction (_, f)) = x
        f s
    
    member x.Strings =
        let (AutoCompleteFunction (s, _)) = x
        s.Force()

let initAutoComplete (names : string seq) =
    let names =
        names
        |> Seq.sort
        |> Array.ofSeq

    let rec getByPrefix range prefix =
        let range =
            match range with
            | Some (first, count) ->
                let first' =
                    seq { first .. first + count - 1 }
                    |> Seq.tryFindIndex (fun pos -> names.[pos].StartsWith prefix)
            
                match first' with
                | None -> None
                | Some first' ->
                    let after =
                        seq { first' .. first + count - 1 }
                        |> Seq.tryFindIndex (fun pos -> not <| names.[pos].StartsWith prefix)
                    match after with
                    | None -> Some (first', first + count - first')
                    | Some after -> Some (first', after - first')
            | None -> None

        let matched_names =
            lazy
                match range with
                | None -> Array.empty
                | Some (first, count) ->
                    Array.sub names first count
        
        let cont next =
            getByPrefix range (prefix + next)

        (matched_names, cont) |> AutoCompleteFunction

    let all_range = Some(0, Array.length names)
    getByPrefix all_range ""