// Serialization of function values
// (Deserialization will only work from within
//  a completely identical binary.)
open System.IO 
open System.Runtime.Serialization.Formatters.Soap

let i = ref 0 
let s = ref "hello world" 

let op () = 
  i := !i + 1 
  s := !s + "!" 

let sf = new SoapFormatter();

let choice = int(System.Console.ReadLine())
if choice = 0 then
 use fs = new FileStream("op.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite)
 sf.Serialize(fs, op);
else
 use fs2 = new FileStream("op.xml", FileMode.Open, FileAccess.Read)
 let op2 = sf.Deserialize(fs2) :?> (unit->unit)
 op2()
 op2()
 printfn "%d %s" !i !s
