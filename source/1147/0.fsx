module SimpleCrypt
open System
open System.Text

let key = [|58uy; 200uy; 140uy; 209uy; 113uy|]
let xorer = (fun i c -> c ^^^ key.[i%key.Length])

let encr (s:string) = s |> Encoding.Default.GetBytes |> Array.mapi xorer |> Convert.ToBase64String
let decr (s:string) = s |> Convert.FromBase64String |> Array.mapi xorer |> Encoding.Default.GetString

(* generate random key

let rnd = System.Random()
let bytes = Array.create 16 0uy
rnd.NextBytes(bytes)
printfn "%A" bytes

*)

(* usage
SimpleCrypt.encr "data to be encrypted"
SimpleCrypt.decr "<encrypted data>"
*)
