open System;
open System.Linq;

let isPalindrome (s : string) =
    (seq s).SequenceEqual(s.Reverse())

let rec getPalindromes (first : DateTime) last acc =
    match first with
    | n when n = last -> acc
    | n when (isPalindrome (n.ToString("ddMMyyyy"))) -> getPalindromes (first.AddDays(1.0)) last (n::acc)
    | _ -> getPalindromes (first.AddDays(1.0)) last acc

let allPalindromes = getPalindromes (new DateTime (1, 1, 1)) (new DateTime(9999, 12, 31)) []


Seq.iter (fun (d : DateTime) -> (printfn "%s" (d.ToShortDateString()))) (seq allPalindromes)

printfn "count: %d" (allPalindromes.Count())