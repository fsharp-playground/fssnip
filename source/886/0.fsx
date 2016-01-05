// [snippet: Coding Kata: Score a Bowling Game]
/// Bowling rules: http://en.wikipedia.org/wiki/Ten-pin_bowling

let score game =
    let rec loop acc turn xs =
        match turn, xs with
            | 0,  _ -> // game over
                acc
            | _, 10::(x::y::_ as xs) -> // Strike : 10 + pins down on the next two throws
                loop (10 + x + y + acc) (turn - 1) xs
            | _, x::y::(z::_ as xs) when x + y = 10 -> // Spare: 10 + pins down in the next throw
                loop (10 + z + acc) (turn - 1) xs
            | _, x::y::xs -> // open frame
                loop (x + y + acc) (turn - 1) xs
            | _ -> 
                failwith "Error in the score"
    loop 0 10 game

let game1 = [10; 10; 10; 10; 10; 10; 10; 10; 10; 10; 10; 10] // Score: 300
// > score game1;;
// val it : int = 300

let game2 = [9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9] // Score: 190
// > score game2;;
// val it : int = 190

let game3 = [10; 1; 9; 1; 2; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 1; 9; 10] // Score: 120
// > score game3;;
// val it : int = 120

let game4 = [5; 2; 3; 4; 4; 2; 6; 1; 8; 0; 0; 9; 2; 7; 2; 3; 8; 1; 3; 3] // Score: 73
// > score game4;;
// val it : int = 73
// [/snippet]
