let fibSeq =
  // trailing tail of fibonacci sequence [tail2, tail1, current]
  let tail2 = ref 1
  let tail1 = ref 2
  Seq.initInfinite (fun index0 ->
    let index = index0 + 1
    if index < 3 then index
    else
      let this = ! tail2 + ! tail1
      tail2 := ! tail1
      tail1 := this
      this
  )

let euler2() =
  let sum = ref 0
  fibSeq |> 
    Seq.filter (fun term -> term % 2 = 0) |>
    Seq.takeWhile (fun term -> term < 4000000) |>
    Seq.iter (fun term -> sum := ! sum + term; ())
  sum