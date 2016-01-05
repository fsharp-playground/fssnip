// [snippet:Import necessary namespaces for GUI programming]
open System
open System.Threading
open System.Windows.Media
open System.Windows.Shapes
open System.Windows.Controls
open Microsoft.TryFSharp

// [/snippet]
// ------------------------------------------------------------------
// [snippet:Helpers for dealing with user interface]

/// Helper function that runs an operation on the UI thread
let guiOperation operation =
  App.Dispatch (fun() -> operation ()) |> ignore

/// Helper function that runs a stateful operation on the user 
/// interface thread repeatedly (with minimal delay) until cancelled 
let guiLoop operation initial =
  let tok = new CancellationTokenSource()
  App.Dispatch (fun() -> 
    let rec loop state = async {
      do! Async.Sleep(1)    
      return! loop (operation state) }
    Async.StartImmediate(loop initial, tok.Token)
    App.Console.CanvasPosition <- CanvasPosition.Right) |> ignore
  tok

// [/snippet]
// ------------------------------------------------------------------
// [snippet:Creating & updating the game grid]

// Configuration of the game of life (map & view sizes)
let actualWidth, actualHeight = 500.0, 500.0
let gameWidth, gameHeight = 40, 40

// Initialize board & colors
let gameView = Array2D.create gameWidth gameHeight null
let black = Color.FromArgb(255uy, 0uy, 0uy, 0uy)
let white = Color.FromArgb(255uy, 250uy, 250uy, 250uy)

// Create shapes representing the game field
guiOperation (fun () ->
  let boxWidth = actualWidth / (float gameWidth)
  let boxHeight = actualHeight / (float gameHeight)
  for x in 0 .. gameWidth - 1 do
    for y in 0 .. gameHeight - 1 do
      let box = Rectangle(Width = boxWidth, Height = boxHeight)
      Canvas.SetTop(box, float x * boxWidth)
      Canvas.SetLeft(box, float y * boxHeight)
      box.Fill <- new SolidColorBrush(white)
      App.Console.Canvas.Children.Add(box)
      gameView.[x, y] <- box
  App.Console.CanvasPosition <- CanvasPosition.Right )

/// Function that shows the new grid state on the screen
let updateGrid data = 
  data |> Array2D.iteri (fun x y v ->
     let color = if v = 1 then black else white
     gameView.[x, y].Fill <- SolidColorBrush(color))

// [/snippet]
// ------------------------------------------------------------------
// [snippet:The game of life!]

/// Count the number of neighbors around game.[x, y]
let computeNeighbours x y (game:int[,]) =
  /// Safely get the value (with wrapping around the field)
  let getValue (x,y) =
    let x = (x + gameWidth) % gameWidth
    let y = (y + gameHeight) % gameHeight
    game.[x,y]
  // Generate offsets, get values & sum the number
  [ for dx in -1 .. 1 do
      for dy in -1 .. 1 do 
        if dx <> 0 || dy <> 0 then 
          yield getValue (x + dx, y + dy) ]
  |> Seq.sum

/// Implements a single step of the game of life
let gameStep board =
  board |> Array2D.mapi (fun x y v ->
    match computeNeighbours x y board with
    | 3 -> 1   // Becomes alive if there are 3 neighbors
    | 2 -> v   // Does not change if there are 2 neighbors
    | _ -> 0 ) // Dies if there is less or more than that

// Initialize the game of life randomly
let rnd = new Random()
let game = Array2D.init gameWidth gameHeight (fun _ _ -> rnd.Next(2))

// Run the simulation - display game & calculate new state in a loop
let it = game |> guiLoop (fun game ->
  updateGrid game
  gameStep game)

// Cancel the evaluation in TryF#
it.Cancel()
// [/snippet]