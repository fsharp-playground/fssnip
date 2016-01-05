let cartesianProduct l1 l2 =
    List.map (fun x -> (List.map (fun y -> x * y) l2)) l1 |> List.concat

let isPalindromic n = 
    let isAnagram (s: string) =
        new string(s.ToCharArray() |> Array.rev) = s
    isAnagram(string n)

cartesianProduct [999..-1..100] [999..-1..100]
|> List.filter isPalindromic
|> List.max
|> printfn "Project Euler Problem 4 Answer: %d"