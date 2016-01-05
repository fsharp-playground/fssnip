open System

// a Bachelor is an identity index and an 
// ordered list of women indicies to approach.
type Bachelor = int * int list

// Some notation:
// wi = woman index (int)
// mi = man index (int)
// mi' = woman's current partner index (int)
// m = man with index and unapproached women indices (Bachelor)
// mSingle = men that are single (Bachelor list)
// wEngaged = engagements from women to men (int, Bachelor)

let funGS (M: _ array) (W: _ array) (comp: _ -> _ -> float) =
  let Windices = [ 0 .. W.Length - 1 ]
  // List of men with women in order of desire  
  let Munproposed = 
    List.init M.Length 
      (fun mi -> 
           let sortFun wi = 1.0 - (comp M.[mi] W.[wi])
           mi, Windices |> List.sortBy sortFun)
  // Recursively solve stable marriages
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
          // The previous suitor (mi') is bested 
          findMarriages 
            (m' :: bachelors) 
            (wEngaged |> Map.add wi m)
        else
          // The current bachelor (mi) lost, better luck next time
          findMarriages 
            (m :: bachelors) 
            wEngaged
  findMarriages Munproposed Map.empty
  // Before returning, remove unproposed lists from man instances  
  |> Map.map (fun wi m -> let mi, _ = m in mi)  
