//Click the Run button to begin!

let __ = "FILL ME IN"

type FILL_ME_IN =
    class end

open System.Windows
open System.Collections
open System.Windows.Controls
open System.Windows.Media
open Microsoft.TryFSharp

type Result =
    | Success
    | Failure of string

let displayResult (result:Result) =
    App.Console.ClearCanvas()
    App.Console.ClearOutput()
    App.Console.CanvasPosition <- CanvasPosition.Right

    let canvas = App.Console.Canvas
    canvas.Background <- new SolidColorBrush(Color.FromArgb(byte 225, byte 170, byte 170, byte 170))

    let grid = new Grid()
    grid.Height <- canvas.ActualHeight
    grid.Width <- canvas.ActualWidth
    canvas.Children.Add grid
    
    let border = new Border()
    border.Margin <- Thickness 7.0
    border.CornerRadius <- CornerRadius 2.0
    border.BorderThickness <- Thickness 5.0
    border.HorizontalAlignment <- HorizontalAlignment.Center
    border.VerticalAlignment <- VerticalAlignment.Center
    grid.Children.Add border


    let stackPanel = new StackPanel()
    stackPanel.Margin <- Thickness 29.0
    stackPanel.Orientation <- Orientation.Vertical
    stackPanel.HorizontalAlignment <- HorizontalAlignment.Center
    stackPanel.VerticalAlignment <- VerticalAlignment.Center

    border.Child <- stackPanel
    
    let outcome = new TextBlock(FontSize = 32.0)
    outcome.HorizontalAlignment <- HorizontalAlignment.Center
    stackPanel.Children.Add outcome

    match result with
    | Success -> 
        outcome.Text <- "Success!"
        border.BorderBrush <- new SolidColorBrush(Colors.Green)
        border.Background <- new SolidColorBrush(Color.FromArgb(byte 225, byte 0, byte 170, byte 0))
    | Failure text ->
        outcome.Text <- "Failure:"
        border.BorderBrush <- new SolidColorBrush(Color.FromArgb(byte 225, byte 170, byte 0, byte 0))
        border.Background <- new SolidColorBrush(Color.FromArgb(byte 225, byte 226, byte 0, byte 0))
        let error = new TextBlock(FontSize = 19.0, TextWrapping = TextWrapping.Wrap)
        error.Text <- text
        stackPanel.Children.Add error

let AssertEquality (x:'a) (y:'b) = 
    let result = 
        match box x with
        | :? IEnumerable as x -> match box y with | :? IEnumerable as y->
            let x = Seq.cast<obj> x
            let y = Seq.cast<obj> y

            let result = Seq.compareWith (fun x y -> if x.Equals(y) then 0 else 1) x y
            0 = result
        | _ -> x.Equals(y)

    if result then
        App.Dispatch (fun () -> displayResult Success)
    else
        let message = sprintf "Expected %A, but received %A instead" x y
        App.Dispatch (fun () -> displayResult (Failure message))
        failwith message

App.Dispatch (fun () -> App.Console.ClearOutput())
App.Dispatch (fun () -> App.Console.LoadFromUrl("http://fssnip.net/raw/bG"))