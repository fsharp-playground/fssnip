let rec split isSplit lines = 
   let b = System.Text.StringBuilder()
   seq {
      for l:string in lines do
         if (isSplit l) then 
            yield b.ToString()
            b.Clear() |> ignore
         else 
            b.Append(l).Append(" ") |> ignore
      if b.Length > 0 then yield b.ToString()
   }
let isEmpty s = s = ""
let logParser logfile = 
   System.IO.File.ReadLines logfile 
   |> split isEmpty

// Test the 'split' method     
let inp = ["hi";"there";"";"aa";"bb";"";"cc"]
let results = inp |> split isEmpty
printfn "%A" results