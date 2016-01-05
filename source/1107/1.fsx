let isPalindrome product = product |> Array.rev = product
let generator =
    seq {
        for x in 990000..999999 do 
            for y in 990000..999999 do 
                yield (x, y, x*y) 
    }

generator
|> PSeq.filter(fun (x, y, z) -> isPalindrome z)
|> Seq.max