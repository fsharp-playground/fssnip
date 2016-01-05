let findWindowBeginnings predicate minWindowSize data =
    if minWindowSize < 2 then invalidArg "minWindowSize" "minWindowSize must be greater than 1"
    ((None, []), data)
    ||> Seq.fold (fun (window, acc) x ->
        if predicate x then
            match window with
            | Some (start, size) -> let size' = size + 1
                                    let acc' = if size' = minWindowSize then start::acc else acc
                                    Some (start, size'), acc'
            | _                  -> Some (x, 1), acc
        else None, acc)
    |> snd
    |> List.rev

// example usage, implementing described use case:
let findHeatwaveBeginnings tempThreshold consecutiveDays data =
    (consecutiveDays, data)
    ||> findWindowBeginnings (snd >> (<=) tempThreshold)
    // alternatively, if one isn't a fan of point-free style code:
    //  findWindowBeginnings (fun (_, maxTemp) -> maxTemp > tempThreshold)
    |> List.map fst