#if INTERACTIVE
#r "PresentationCore.dll"
#r "PresentationFramework.dll"
#r "WindowsBase.dll"
#endif

open System.ComponentModel
open Microsoft.FSharp.Quotations.Patterns

type ObservableObject () =
    let propertyChanged = 
        Event<PropertyChangedEventHandler,PropertyChangedEventArgs>()
    let getPropertyName = function 
        | PropertyGet(_,pi,_) -> pi.Name
        | _ -> invalidOp "Expecting property getter expression"
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = propertyChanged.Publish
    member this.NotifyPropertyChanged propertyName = 
        propertyChanged.Trigger(this,PropertyChangedEventArgs(propertyName))
    member this.NotifyPropertyChanged quotation = 
        quotation |> getPropertyName |> this.NotifyPropertyChanged

type MessageViewModel () =
    inherit ObservableObject()
    let mutable text = ""
    member this.Message
        with get () = text
        and set value = 
            text <- value
            this.NotifyPropertyChanged <@ this.Message @>

open System.Windows
open System.Windows.Controls
open System.Windows.Markup

let xaml = 
    @"<Window xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
       <TextBlock Text='{Binding Message}'/>
      </Window>"
let view = XamlReader.Parse(xaml) :?> Window
do  view.DataContext <- MessageViewModel(Message="Hello World")
#if INTERACTIVE
do  view.Show() |> ignore
#else
[<System.STAThread>]
do  (Application()).Run(view) |> ignore
#endif