//[snippet:Implementation]
open System
type Placeholder () =
  static member inline (*) (_:Placeholder, rhs) = fun lhs -> lhs * rhs
  static member inline (/) (_:Placeholder, rhs) = fun lhs -> lhs / rhs
  static member inline (+) (_:Placeholder, rhs) = fun lhs -> lhs + rhs
  static member inline (-) (_:Placeholder, rhs) = fun lhs -> lhs - rhs
  static member inline (%) (_:Placeholder, rhs) = fun lhs -> lhs % rhs

let __1 = Placeholder ()
let _1 binOp rhs = fun lhs -> binOp lhs rhs

let printHex (n:int) = (*[omit:(...)]*)
  "0b" + Convert.ToString(n,2).PadLeft(8,'0')
  |> printfn "%s"
  n (*[/omit]*)
//[/snippet]

// [snippet:Usage]
0xff             |> printHex
|> __1 % 0x10    |> printHex
|> __1 * 0x10    |> printHex
|> __1 / 0x04    |> printHex
|> __1 - 0x30    |> printHex
|> __1 + 0xc0    |> printHex
|> _1 (|||) 0x33 |> printHex
|> _1 (&&&) 0x55 |> printHex
|> _1 (<<<) 1    |> printHex
|> _1 (>>>) 1    |> printHex
|> ignore
// [/snippet]

// [snippet:Result]
//0b11111111
//0b00001111
//0b11110000
//0b00111100
//0b00001100
//0b11001100
//0b11111111
//0b01010101
//0b10101010
//0b01010101
// [/snippet]