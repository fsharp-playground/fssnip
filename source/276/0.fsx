let subs (s:string) =             
   let lim = s.Length - 1        

   [ for ii in [0..lim] do
      for jj in [0..lim] do  // can these two be combined?
         if ii < jj then
            yield s.[ii..jj] ]

let isPalindrome s =
   let s' = Array.ofSeq s
   (s' = Array.rev s')

let palindromes s =
   printf "Palindromes within \"%s\": \t" s
   s |> subs |> List.filter isPalindrome |> List.map (printf "%s ") |> ignore
   printf "\n"

palindromes "abcbcbx"
palindromes "abcba"