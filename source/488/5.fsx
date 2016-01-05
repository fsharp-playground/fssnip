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
      else // She's engaged, fight for love!
        let mi' = Wmarriages.[wi]
        if comp W.[wi] M.[mi] > comp W.[wi] M.[mi'] then
          Mmarriages.[mi] <- wi
          Wmarriages.[wi] <- mi
          Mmarriages.[mi'] <- aloneVal
  Wmarriages 
  // Make the output the same as the functional version
  |> Array.mapi (fun wi mi -> wi, mi) 
  |> Array.filter (fun (wi, mi) -> mi <> aloneVal)
  |> Map.ofArray

// a Bachelor is an identity index and an 
// ordered list of women indicies to approach.
type Bachelor = int * int list

// Some notation:
// wi = woman index (int)
// mi = man index (int)
// mi' = woman's current partner index
// m = man with index and unapproached women indices (Bachelor)
// mSingle = Men that are single (Bachelor list)
// wEngaged = A map of engagements from women to men (wi, Bachelor)

let funGS (M: _ array) (W: _ array) (comp: _ -> _ -> float) =
  let Windices = [ 0 .. W.Length - 1 ]
  // List of men with women in order of desire
  let Munproposed = 
    List.init M.Length 
      (fun mi -> mi, Windices 
                     |> List.sortBy (fun wi -> 1.0 - (comp M.[mi] W.[wi])))
  let rec findMarriages mSingle wEngaged =
    match mSingle with
    // No single guys left with desired women, we're done
    | [] -> wEngaged
    // Guy is out of luck, remove from singles
    | (mi, []) :: bachelors -> findMarriages bachelors wEngaged
    // He's got options!
    | (mi, wi :: rest) :: bachelors -> 
      let m = mi, rest
      match wEngaged |> Map.tryFind wi with
      // She's single! m is now engaged!
      | None -> findMarriages bachelors (wEngaged |> Map.add wi m)
      // She's already engaged, let the best man win!
      | Some (m') -> 
        let mi', _ = m'
        if comp W.[wi] M.[mi] > comp W.[wi] M.[mi'] then 
          // Congrats mi, he is now engaged to wi
          // The previous suitor (mi') is bested and is back in the dating game
          findMarriages (m' :: bachelors) (wEngaged |> Map.add wi m)
        else
          // The current bachelor (mi) lost, better luck next time
          findMarriages (m :: bachelors) wEngaged
  findMarriages Munproposed Map.empty
  // Before returning, remove unproposed lists from man instances  
  |> Map.map (fun wi m -> let mi, _ = m in mi)  
