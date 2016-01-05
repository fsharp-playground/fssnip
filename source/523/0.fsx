let wl = System.IO.File.ReadAllLines("C:\Users\pc\Desktop\wordlist.txt")

module caesar =
  let cipher (k: int) (m: string) =
    Seq.toArray(m)
    |> Array.map (fun c -> 
      if int c = 32 
        then c
      else ((int c - 97 + k) % 26 + 97) |> char)
      
    |> (fun s -> new string(s))

  let encrypt k = cipher k
  let decrypt k = cipher (26 - k)

let checkWl d = 
   Array.filter(fun x -> d = x)wl

(* below here is highly incorrect, but it works *)
let run (d: string) = 
  Array.map(fun x -> caesar.decrypt x (d.ToLower()))[|1..25|]  
  |> Array.map (fun x -> 
  (x.Split ' ')
    |> Array.map (fun x -> checkWl x))
    |> Array.concat 
    |> Array.concat (*woops * 2 this is highly uneconomic*)
    |> Array.iter(fun x -> printfn "%s" x)
    
run (caesar.encrypt 13 "today is cold")
