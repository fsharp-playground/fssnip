open System

#r "FSharp.PowerPack.Parallel.Seq";;
open Microsoft.FSharp.Collections

let MD5 = new MD5CryptoServiceProvider()

let alignMD5 (md : string) = 
  md.Replace("-","").ToLower() 

(* new md5, much faster, no create()*)
let md5 (s : string) = 
  s, Encoding.UTF8.GetBytes s
  |> MD5.ComputeHash
  |> BitConverter.ToString
  |> alignMD5

(* perm code from
   http://stackoverflow.com/questions/4495597/combinations-and-permutations-in-f 

   Generates the cartesian outer product of a list of sequences LL *)
let rec outerProduct = function
  | []    -> Seq.singleton []
  | L::Ls -> L |> Seq.collect (fun x -> 
                  outerProduct Ls |> Seq.map (fun L -> x::L))

(* Generates all n-element combination from a list L *)
let getPermsWithRep n L = 
    List.replicate n L |> outerProduct  
 
let listToStr xs = 
   List.toArray xs |> fun c -> new string (c)

let crmd md5' (charset : string) n = 

  getPermsWithRep n (charset |> Seq.toList)
  |> PSeq.map (PSeq.toList >> listToStr >> md5) 

  |> Seq.filter (fun e -> snd e = md5')
  
(* ("c1a2bb1", "34a79dcbe2670a58abfa4d502ae0fe77") *)
let plaintext = "c1a2bb1"
let md = md5 plaintext

crmd (snd md) "abc123" 7 