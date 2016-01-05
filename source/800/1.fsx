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
  // TODO: Calculate the number of neighbours around (x, y)
  0

/// Implements a single step of the game of life
let gameStep board =
  // TODO: Implement the game logic 
  board

// TODO: Initialize the game of life randomly
let game = Array2D.init gameWidth gameHeight (fun _ _ -> 0)

// Display the current state of the game
guiOperation (fun () ->
  updateGrid game)


// Run the simulation - display game & calculate new state in a loop
let it = game |> guiLoop (fun game ->
  updateGrid game
  gameStep game)

// Cancel the evaluation in TryF#
it.Cancel()
// [/snippet]