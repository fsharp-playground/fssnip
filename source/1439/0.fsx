let FromBooleans (bools:bool []) =
    let bit b = if b then 1uy else 0uy
    let folder (acc,lst,sh) item = if sh >= 8 then (bit(item),acc::lst,1) 
                                   else (acc ||| (bit(item)<<<sh),lst,sh+1)
    let (acc,lst,_) = bools |> Seq.fold folder (0uy,[],0)
    acc::lst |> Array.ofList |> Array.rev