#time

// from codingdojo.org/cgi-bin/wiki.pl?KataBankOCR

// fax gives client number in the form
// 3 x 27 characters either _ or |

let numbers = @"
 _     _  _       _   _  _   _   _ 
| | |  _| _| |_| |_  |_   | |_| |_|
|_| | |_  _|   |  _| |_|  | |_|  _|
"

let numArray2D =
    numbers.Split('\n').[1..3]
    |> Array.map (fun str -> str.ToCharArray())
    |> array2D

let numArray =
    [|
        numArray2D.[*,0..2]
        numArray2D.[*,4..4]
        numArray2D.[*,6..8]
        numArray2D.[*,10..11]
        numArray2D.[*,13..15]
        numArray2D.[*,17..19]
        numArray2D.[*,21..23]
        numArray2D.[*,25..26]
        numArray2D.[*,28..30]
        numArray2D.[*,32..34]
    |]

let numMap =
    numArray
    |> Array.mapi ( fun i num -> num , i )
    |> dict

numMap.TryGetValue( numArray.[3] )

let blank =
    [[' ']
     [' ']
     [' ']] |> array2D


let chars2nums (chars:string) =
    let width =
        chars.Split('\n')
        |> Array.map (fun str -> str.Length)
        |> Array.max

    let depth =
        chars.Split('\n').Length

    let chars2D =
        chars.Split('\n').[1..3]
        |> Array.map (fun str -> str.ToCharArray())
        |> array2D
    
    let width = 
        chars2D |> Array2D.length2
    
    [ 0 .. width-1 ]
        |> List.fold (fun (cursor : int , result : int list) newCursor ->
            //printfn "at cursor %i and newCursor %i" cursor newCursor
            let block = chars2D.[*,cursor .. newCursor]
            let spaceAfterBlock = if newCursor = width-1 then blank else chars2D.[*, newCursor+1 .. newCursor+1 ]
            match (block = blank) with
            | true -> ( newCursor+1 , result )
            | _ ->
                match ( numMap.TryGetValue(block) , spaceAfterBlock = blank ) with
                | (true,x) , true ->
                    //printfn "   I found a number!!! it's : %i" x
                    (newCursor+1 , x :: result )
                | _ -> ( cursor , result )
            )
            ( 0 , [] )
        |> snd
        |> List.rev



let chars = @"
   _  _       _   _  _   _   _ 
|  _| _| |_| |_  |_   | |_| |_|
| |_  _|   |  _| |_|  | |_|  _|
" // first line is just empty, from line 1 to 3 is where the numbers are

chars |> chars2nums
