let isPalindrome product = product |> Array.rev = product
Async.Parallel [for x in 900..999 do for y in 900..999 do yield async { if isPalindrome x then return x }]