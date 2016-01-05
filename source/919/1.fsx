module Program

  let puzzle (s: string, t: string) : string =
    let sArr = Array.ofSeq s |> Array.map(fun x -> x.ToString())
    let tArr = Array.ofSeq t |> Array.map(fun y -> y.ToString())
    let result arr1 arr2 =
      let len1 = arr1 |> Array.length
      let len2 = arr2 |> Array.length
      let less = min len1 len2
      let count = abs (len1 - len2)
      let fold = Array.fold(fun s e -> s+e) ""
      let first = [| for i=0 to less-1 do yield sArr.[i] + tArr.[i]|] |> fold
      let rest  = Array.init count (fun i -> if less = len1 then arr2.[i+less] else arr1.[i+less]) |> fold
      first + rest
    result sArr tArr

  // "eafbgchdijk"
  puzzle ("efghijk", "abcd")

  // "aebfcgdhijk"
  puzzle ("abcd", "efghijk")