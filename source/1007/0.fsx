open System

let fromHex (s:string) = 
  s
  |> Seq.windowed 2
  |> Seq.mapi (fun i j -> (i,j))
  |> Seq.filter (fun (i,j) -> i % 2=0)
  |> Seq.map (fun (_,j) -> Byte.Parse(new System.String(j),System.Globalization.NumberStyles.AllowHexSpecifier))
  |> Array.ofSeq