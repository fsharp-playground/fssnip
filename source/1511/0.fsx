let inOp op f = Seq.groupBy (fun (x, y) -> op x y) >> Seq.filter f >> Seq.collect snd
let singleCase op = inOp op (snd >> Seq.length >> (=) 1)

let all = [for x = 2 to 49 do for y = x + 1 to 99 - x do yield x, y]
let spoilP = all |> singleCase (*) |> Set.ofSeq
all |> inOp (+) (snd >> Set.ofSeq >> Set.intersect spoilP >> Set.isEmpty)
|> singleCase (*) |> singleCase (+) |> Seq.exactlyOne