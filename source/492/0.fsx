open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

let defaultSize = 10 , 10
let bombRate = 0.11

// functions
let rand = let rand = new Random() in rand.NextDouble

let neighbours (width,height) (pos:int) = seq {
  let range x bound = [max 0 (x-1) .. min bound (x+1)]
  for x in range (pos%width) (width-1) do
    for y in range (pos/width) (height-1) do
      let p = x + y * width in if p <> pos then yield p }

// WPF
let appendTo (panel:Panel) uie = 
  panel.Children.Add uie |> ignore
  uie
let dockTo(panel:DockPanel) dock (uie:#UIElement) =
  DockPanel.SetDock(uie,dock)
  appendTo panel uie

type MyComboBox(text) as this = 
  inherit StackPanel(Orientation=Orientation.Vertical)
  let textBlock = new TextBlock(Text=text,Foreground=Brushes.White) |> appendTo this
  let listBox = new ListBox() |> appendTo this
  do for x in 4 .. 2 .. 30 do listBox.Items.Add x |> ignore
     listBox.SelectedItem <- 10
  member __.Value = listBox.SelectedItem :?> int

// view
let window = Window(Background=Brushes.BlueViolet,ResizeMode=ResizeMode.NoResize)
window.Title <- "FineSweeper"

let dpMain = new DockPanel()
window.Content <- dpMain

let spsize = new StackPanel(Orientation=Orientation.Horizontal) |> dockTo dpMain Dock.Left

let comboBoxWidth = new MyComboBox("X") |> appendTo spsize
let comboBoxHeight = new MyComboBox("Y") |> appendTo spsize

let buttonInit = new Button(Content="Replay") |> dockTo dpMain Dock.Top

let canvas = new Canvas() |> appendTo dpMain

let initButtons(lenX,lenY) =
  let f(pos:int) =
    let b = new Button(Width=20. , Height=20.)
    Canvas.SetLeft(b, float(pos%lenX*20))
    Canvas.SetTop(b, float(pos/lenX*20))
    canvas.Children.Add b|> ignore
    b
  Array.init (lenX*lenY) f

// model
type State = Normal | Pushed | Flaged
type Square = 
  {Button:Button; mutable state:State; IsBomb:bool; BombNum:int; Neighbours:seq<Square> } with 
  member this.State
    with get() = this.state
    and set value =
      this.state <- value
      let pushedText = if this.IsBomb then "※" else string this.BombNum
      match value with 
        |Normal->true,"" |Pushed->false,pushedText |Flaged->true, "¶"
      |> fun (isEnabled,text) ->
        this.Button.IsEnabled <- isEnabled
        this.Button.Content <- text

#nowarn "40"
let initSquares (lenX,lenY) : Square [] =
  let buttons = initButtons (lenX,lenY)
  let bombs : bool[] = Array.init (lenX*lenY) (fun _ -> rand() < bombRate)
  let rec squares =
    Array.init (lenX*lenY) (fun p->
    let neighbours = neighbours(lenX,lenY) p
    let bombNum = neighbours |> Seq.filter(fun p -> bombs.[p]) |> Seq.length
    let neighbours = neighbours |> Seq.map(fun p -> squares.[p])
    {Button=buttons.[p]; state=Normal; IsBomb=bombs.[p]; BombNum=bombNum; Neighbours=neighbours})
  squares

let changeSqs sq =
  let examined = ResizeArray()
  let rec f(s:Square) =
    if not(examined.Contains s) && not(s.IsBomb) then
      examined.Add s
      if s.BombNum = 0 then Seq.iter f s.Neighbours
  f sq ; examined

// controller      
let onInit () =
  canvas.Children.Clear()
  let lenX,lenY = comboBoxWidth.Value , comboBoxHeight.Value
  let squares = initSquares(lenX,lenY)
  
  let onGameEnd text = 
    MessageBox.Show text |> ignore
    for sq in squares do sq.State <- Pushed
  let onCheck () = 
    if squares |> Seq.forall(fun sq->sq.State<>Normal || sq.IsBomb) then onGameEnd "You win!!"
  let onClick(sq:Square) = 
    if sq.State = Normal then 
      if sq.IsBomb then onGameEnd "You lose!!" else
      for sq in changeSqs sq do sq.State <- Pushed
      onCheck()

  let onRightClick (sq:Square) = 
    sq.State <- if sq.State = Normal then Flaged else Normal

  for {Button=b} as sq in squares do
    b.Click.Add(fun _ ->  onClick sq )
    b.MouseRightButtonDown.Add(fun _ -> onRightClick sq ; onCheck())
  window.Width <- lenX * 20 + 55 |> float
  window.Height <- lenY * 20 + 60 |> float

buttonInit.Click.Add(fun _-> onInit())

[<EntryPoint>][<STAThread>]
let main _ = 
  onInit()   
  (Application()).Run window