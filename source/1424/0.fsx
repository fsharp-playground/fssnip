module Hash
 
let jenkins (s:string) =
    let h = 
        System.Text.Encoding.ASCII.GetBytes(s) |> Array.fold (fun ac b ->
            let ac = ac + (uint32 b)
            let ac = ac + (ac <<< 10)
            ac ^^^ (ac >>> 6)
        ) 0u
    let h = h + (h <<< 3)
    let h = h ^^^ (h <<< 11)
    h + (h <<< 15)