// Two perfect logicians, S and P, are told that integers x and y have been chosen
// such that 1 < x < y and x+y < 100. S is given the value x+y and P is given the
// value xy. They then have the following conversation.
//
//     P:  I cannot determine the two numbers.
//     S:  I knew that.
//     P:  Now I can determine them.
//     S:  So can I.
//
// Given that the above statements are true, what are the two numbers?

let inOp op f = Seq.groupBy ((<||) op) >> Seq.map snd >> Seq.filter f >> Seq.concat
let singleCase op = inOp op (Seq.length >> (=) 1)

let all = [for x = 2 to 49 do for y = x + 1 to 99 - x do yield x, y]
let spoilP = all |> singleCase (*) |> Set.ofSeq
all |> inOp (+) (Set.ofSeq >> Set.intersect spoilP >> Set.isEmpty)
|> singleCase (*) |> singleCase (+) |> Seq.exactlyOne