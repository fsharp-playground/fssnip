open System
open FSharp.Extensions.Joinads

/// state of the square Empty or Occupied by O or X.
type Square = Empty | O | X 
/// turn to move 
let mutable turn = O
/// squares of the game
let squares = [| for _ in 0..8 -> Empty |]

/// Standard .NET delegate is needed to use Async.AwaitEvent
type PutEventHandler = delegate of obj * Square -> unit
/// events which fired when the corresponding square is occupied by any player  
let putEvents = [| for _ in 0..8 -> Event<PutEventHandler,Square>() |]

/// print the turn and status of the squares
let printPosition () =
  String.concat "\n-+-+-\n" [ 
    for y in 0..2 ->
      String.concat "|" [
        for x in 0..2 ->
          match squares.[x+y*3] with
          | Empty -> " "
          | O -> "O"
          | X -> "X" 
      ]
  ] 
  |> printfn "%s"
  printfn "(the side to next move is %A)\n" turn

/// swap turn to move  
let swapTurn () = 
  turn <- match turn with O -> X | X -> O | _ -> failwith "invalid turn!!"

/// put piece on the corresponding square of the given index
let putOn index =
  // is inside the board ?
  if 0 <= index && index < 9 then
    // is the target square empty ?
    if squares.[index] = Empty then
      squares.[index] <- turn
      putEvents.[index].Trigger(null,turn)
      swapTurn ()
      printPosition ()        
    else
      printfn "index %d is already occupied by %A\n" index squares.[index]
  else
    printfn "square must be between 1 and 9\n"

/// clear squares to initiazlize game position
let clearSquares () =
  for index in 0..8 do squares.[index] <- Empty
  
/// Judge win of the specified player.
/// return the line occupied all squares by the same player's mark
let judgeWinOf (turn:Square) = async {
  let putted index = 
    putEvents.[index].Publish
    |> Event.filter(fun square -> square = turn)
    |> Async.AwaitEvent
  match!                             
    putted 0, putted 1, putted 2,
    putted 3, putted 4, putted 5,
    putted 6, putted 7, putted 8 with
  | _,_,_,?,?,?,?,?,? -> return "0-1-2"
  | ?,?,?,_,_,_,?,?,? -> return "3-4-5"
  | ?,?,?,?,?,?,_,_,_ -> return "6-7-8"
  | _,?,?,_,?,?,_,?,? -> return "0-3-6"
  | ?,_,?,?,_,?,?,_,? -> return "1-4-7"
  | ?,?,_,?,?,_,?,?,_ -> return "2-5-8"
  | _,?,?,?,_,?,?,?,_ -> return "0-4-8"
  | ?,?,_,?,_,?,_,?,? -> return "2-4-6"
  } 

/// play all games of given books
let playTicTacToeGames books =
  for book in books do
    /// put marks according to the book
    let putting = async {
      for square in book do
        printfn "** put on square %d" square
        putOn square
        do! Async.Sleep 100
      }
    printfn "****\n**** Start a new game!!\n****\n"
    async {
      // game ends when player X or O won or all marks in book are putted
      match! judgeWinOf X , judgeWinOf O , putting with
      | line, ?,    ? -> printfn "** Player X won the game!!(line=%s)\n" line
      | ?,    line, ? -> printfn "** Player O won the game!!(line=%s)\n" line
      | ?,    ?,    _ -> printfn "** Game draw!!\n"
      clearSquares ()
    }
    |> Async.RunSynchronously

let books = [[0;3;1;4;6;7;2];[0;4;8;1;7;6;2;5;3]]
playTicTacToeGames books // start games