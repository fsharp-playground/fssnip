// Learn more about F# at http://fsharp.net
open System.IO

let value = 300

printfn "%i" value

let bA = System.BitConverter.GetBytes(value)

for i in bA do
    printfn "%i" i
    
let ms = new MemoryStream(bA)

printfn "%i" ms.Position

let rB = ms.ReadByte()
//ms.Position <- ms.Position - 1L

printfn "%i" ms.Position
printfn "%i" rB

ignore(System.Console.ReadKey(true))
