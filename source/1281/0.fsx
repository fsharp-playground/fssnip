let zeroToNineText = """
 _     _  _     _  _  _  _  _ 
| |  | _| _||_||_ |_   ||_||_|
|_|  ||_  _|  | _||_|  ||_| _|                          
"""

let getDigit (text:string) digit = 
  [| for line in text.Split('\n').[1..3] -> line.Substring (3*digit, 3) |]

let zeroToNine = List.map (getDigit zeroToNineText) [0..9]

let toNumber number =
  Seq.init 9 (fun i -> Seq.findIndex ((=) (getDigit number i)) zeroToNine)
  |> Seq.fold(fun state x -> x + 10 * state) 0
        
let accountNo = """
    _  _  _  _  _  _     _ 
|_||_|| || ||_   |  |  ||_ 
  | _||_||_||_|  |  |  | _| 
"""

let n = toNumber accountNo 