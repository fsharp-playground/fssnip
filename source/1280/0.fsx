open System

// Defs
let zero = """
 _ 
| |
|_|"""

let one = """
   
  |
  | 
"""
let two = """
 _ 
 _|
|_ """ 

let three = """
 _ 
 _|
 _|"""
   
let four = """
   
|_|
  |"""

let five = """
 _ 
|_ 
 _|"""
let six = """
 _ 
|_ 
|_|"""
let seven = """
 _ 
  |
  |"""
let eight = """
 _ 
|_|
|_|"""
let nine = """
 _ 
|_|
 _|"""

// Part 1
let inline numberMap (n: string) =
    let n' = n.[1..].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None)
    Array2D.init 3 3 (fun i j -> n'.[i].[j])
    
let numbers = 
    [|zero; one; two; three; four; five; six; seven; eight; nine|]
    |> Array.mapi (fun i n -> numberMap n, i) |> dict
    
let usercase1 = """
    _  _     _  _  _  _  _ 
  | _| _||_||_ |_   ||_||_|
  ||_  _|  | _||_|  ||_| _|""".[1..]


let parseNumbers (numberStr: string) =
    let charArr = numberStr.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                  |> Array.map (fun i -> i.ToCharArray())
    let charArr2d = Array2D.init (charArr.Length) (charArr.[0].Length)
                        (fun i j -> charArr.[i].[j])
    [|
        
        for j = 0 to (charArr.[1].Length - 1) / 3 do
            let v = 
                try 
                    let idx = j*3 in charArr2d.[0..2, idx.. idx+2] 
                 with ex -> failwithf "%i" j
            yield v
    |] |> Array.map (fun v -> numbers.[v])

// Part 2 
let doesChecksum (acc: int []) =
    let summed = acc |> Array.rev |> Array.mapi (fun i v -> (i+1) * v) |> Array.sum
    summed % 11 = 0
