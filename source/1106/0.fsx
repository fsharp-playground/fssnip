let isPalindrome product = product |> Array.rev = product
[|for x in 0 .. 99 do for y in 0 .. 99 do yield (x + 900, y + 900, (x+900)*(y+900))|]
|> Array.sortBy(fun (x, y, product) -> -product)
|> Array.find(fun (x, y, product) -> isPalindrome (product.ToString().ToCharArray()))