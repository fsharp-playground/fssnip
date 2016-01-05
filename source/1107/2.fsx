let isPalindrome (x : int) = let numberArray = x.ToString().ToCharArray()
                             numberArray = Array.rev numberArray
 
let numberArray = Array.Parallel.init 100 ((+)900)
let result = numberArray |> Array.Parallel.collect(fun x -> [| for y in numberArray do let product = x*y
                                                                                       if isPalindrome product then yield product|])
             |> Array.max