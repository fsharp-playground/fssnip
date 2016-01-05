open System
open System.Numerics

let rec fft = function
  | []  -> []
  | [x] -> [x] 
  | x ->
    x
    |> List.mapi (fun i c -> i % 2 = 0, c)
    |> List.partition fst
    |> fun (even, odd) -> fft (List.map snd even), fft (List.map snd odd)
    ||> List.mapi2 (fun i even odd -> 
        let btf = odd * Complex.FromPolarCoordinates(1., -2. * Math.PI * (float i / float x.Length ))
        even + btf, even - btf)
    |> List.unzip
    ||> List.append
  
// use

let input = [for x in 0. .. 15. -> cos(x)  + cos(4.0 * x)]
    
let output = 
    input
    |> List.map (fun r -> Complex(r, 0.)) 
    |> fft
    |> List.map (fun c -> c.Real)
    