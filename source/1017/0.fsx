let frequencySort l f =
  let fSort (d:System.Collections.Generic.Dictionary<_,_>) =
      let lens = l |> List.map List.length
      lens |> List.iter (fun i -> if d.ContainsKey(i) then d.[i] <- d.[i] + 1 else d.[i] <- 1)
      l |> List.sortBy (fun a -> d.[a.Length])
  fSort (System.Collections.Generic.Dictionary()) 

let test = frequencySort  [ ["a";"b";"c"]; ["d";"e"]; ["f";"g";"h"]; ["d";"e"];
                            ["i";"j";"k";"l"]; ["m";"n"]; ["o"] ]
                          List.length

// prints
// [["i"; "j"; "k"; "l"]; ["o"]; ["a"; "b"; "c"]; ["f"; "g"; "h"]; ["d"; "e"];
// ["d"; "e"]; ["m"; "n"]]