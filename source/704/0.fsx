open System.Reflection
open System.Reflection.Emit

//type generic (x) = member this.funct = 1

let opcodes = 
  typeof<OpCodes>.GetFields() 
  |> Seq.map (fun x -> x.GetValue() :?> OpCode |> fun o -> o.Value, o.Name)
  |> Map.ofSeq

let find_opcode (b : byte array) =
  Seq.map (fun bs -> Map.tryFind (int16 bs) opcodes) b
  
let getMSIL F =  
  let t1 = F.GetType().GetFields(BindingFlags.NonPublic ||| BindingFlags.Instance) 
           |> Array.map (fun field -> field.Name)
  
  t1.GetType().GetMethods()
  |> Seq.map (fun mb -> try mb.GetMethodBody().GetILAsByteArray() with ex -> [|0uy|])
  |> Seq.map find_opcode
  |> Seq.concat
  |> Seq.filter ((<>) None) //lol
  
let printMSIL F =
  getMSIL F |> Seq.iter (fun x-> printfn "%s" x.Value) 
  //prints about 1200 loc msil? surely that isnt right for something like let x = 1 printMSIL x, guess its printing whole assembly?
