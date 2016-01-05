let zeroToNineText = """
 _     _  _     _  _  _  _  _ 
| |  | _| _||_||_ |_   ||_||_|
|_|  ||_  _|  | _||_|  ||_| _|                          
"""

let getDigits (source:string) len =
    let lines = source.Split([|'\n'|]).[1..3]
    [|for digit in 0..len-1 ->
        let index = digit*3
        [|for line in lines ->            
            line.[index..index+2]
        |]
        |> String.concat "\n"
    |]

let zeroToNine = getDigits zeroToNineText 10

let toNumber text =
    getDigits text 9
    |> Array.rev 
    |> Array.fold (fun (result,tens) digit  ->
        let n = zeroToNine |> Array.findIndex ((=) digit)
        result + n * tens, tens * 10
    ) (0,1)
    |> fst
        
let accountNo = """
    _  _  _  _  _  _     _ 
|_||_|| || ||_   |  |  ||_ 
  | _||_||_||_|  |  |  | _| 
"""

let n = toNumber accountNo