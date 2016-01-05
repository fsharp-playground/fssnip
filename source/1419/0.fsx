//(*) Problem 25-- Generate a random permutation of the elements of a list.
//Example in F#-- 
//rnd_permu <| List.ofSeq "abcdef";;
//val it : char list = ['b'; 'c'; 'd'; 'f'; 'e'; 'a']
 
let rnd_permu (lst : 'a list) =
    let rnd = System.Random()
    [for i in (Seq.initInfinite (fun _ ->rnd.Next(0,(lst.Length-1))) |> Seq.distinct |> Seq.take lst.Length) -> lst.[i]]
