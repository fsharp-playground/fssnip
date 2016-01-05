module CircularPrint

open FsUnit
open NUnit.Framework

type Direction = int * int -> int * int

type Directions = Direction []

let right (x,y) = (x + 1, y)
let down  (x,y) = (x, y + 1)
let left  (x,y) = (x - 1, y)
let up    (x,y) = (x, y - 1)

let directions : Directions = [| right; down; left; up; |]

let board n = 
    let count = ref 0
    Array2D.init n n (fun x y -> count := !count + 1; !count)
     
let printBoard (directions : Directions) (board: int[,]) = 
    seq {
        let (@) (twoDArr : 'a[,]) (x,y) = twoDArr.[y,x]

        let length = Array2D.length1 board

        let rec print pos moveNum bound =         
            seq{               
       
                let boundsCheck (x, y) = 
                    let ``end`` = length - bound 
                    let start = bound
       
                    x < ``end`` && y < ``end`` && x >= start && y >= start

                let direction n = directions.[n % (length - 1)]

                let move = moveNum |> direction 
        
                if boundsCheck pos then 
                    yield board @ pos 
                
                let next = pos |> move 
                            
                // go till the next guy hits the past bound
                if boundsCheck next then                    
                    yield! print next moveNum bound

                // if the next goes beyond the bound, shift directions on the original
                // until we've moved N times, covering all elements of the board
                else if moveNum < length then

                    yield! print pos (moveNum + 1) bound    
            }       

        for layer in 0.. Array2D.length1 board do
            yield! print (layer, layer) 0 layer
    } |> Seq.distinct


[<Test>]
let testSquare4 () =     
    board 4
        |> printBoard directions
        |> Seq.toList
        |> should equal [1; 2; 3; 4; 8; 12; 16; 15; 14; 13; 6; 7; 11; 10]


[<Test>]
let testSquare5 () = 
    
    board 5
        |> printBoard directions
        |> Seq.toList
        |> should equal [1; 2; 3; 4; 5; 10; 15; 20; 25; 24; 23; 22; 21; 16; 11; 6; 7; 8; 9; 14; 19; 18; 17; 12; 13]
       