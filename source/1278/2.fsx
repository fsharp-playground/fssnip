let zeroToNineText = """
 _     _  _     _  _  _  _  _ 
| |  | _| _||_||_ |_   ||_||_|
|_|  ||_  _|  | _||_|  ||_| _|                          
"""

let getDigits (source:string) len =
    let lines = source.Split([|'\n'|]).[1..3]
    [for digit in 0..len-1 ->
        let index = digit*3
        [for line in lines -> line.[index..index+2]]
    ]

let zeroToNine = getDigits zeroToNineText 10

let toNumber text =
    getDigits text 9
    |> List.fold (fun acc digit ->
        let n = zeroToNine |> List.findIndex ((=) digit)
        acc * 10 + n
    ) 0
        
let accountText = """
    _  _  _  _  _  _     _ 
|_||_|| || ||_   |  |  ||_ 
  | _||_||_||_|  |  |  | _| 
"""

let n = toNumber accountText