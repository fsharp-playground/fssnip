open System

let impGS (M: _ array) (W: _ array) (comp: _ -> _ -> float) =
    let aloneVal = Int32.MaxValue
    // Everyone starts single
    let Mmarriages = Array.create M.Length aloneVal
    let Wmarriages = Array.create W.Length aloneVal
    // Each man builds his list of women, in order
    let Windices = [ 0 .. W.Length - 1 ]
    let rankWomen mi = 
        Windices 
        |> List.sortBy (fun wi -> 1.0 - (comp M.[mi] W.[wi]))
    let Munproposed = Array.init M.Length rankWomen 
    // Generates the next match if one is available
    // also maintains the state of Munproposed
    let getNextBachelorWithProspects () = 
        let mutable mi = Munproposed.Length
        let mutable wi = -1
        while mi > 0 && wi = -1 do
            mi <- mi - 1
            match Mmarriages.[mi], Munproposed.[mi] with
            | current, head :: rest when current = aloneVal -> 
                wi <- head
                Munproposed.[mi] <- rest
            | _ -> ()
        mi, wi
    let mutable keepLooking = true
    while keepLooking do
        match getNextBachelorWithProspects ()  with
        // No single men with prospects left, we're done
        | _, -1 -> keepLooking <- false 
        // A lonely guy
        | mi, wi -> 
            if (Wmarriages.[wi] = aloneVal) then // She's single!
                Mmarriages.[mi] <- wi
                Wmarriages.[wi] <- mi
                printfn "%s is now married to %s" W.[wi] M.[mi] 
            else // She's engaged, fight for love!
                let mi' = Wmarriages.[wi]
                if comp W.[wi] M.[mi] > comp W.[wi] M.[mi'] then
                    Mmarriages.[mi] <- wi
                    Wmarriages.[wi] <- mi
                    Mmarriages.[mi'] <- aloneVal
    Wmarriages 
    // Here to make the output the same as the functional version
    |> Array.mapi (fun wi mi -> wi, mi) 
    |> Array.filter (fun (wi, mi) -> mi <> aloneVal)
    |> Map.ofArray