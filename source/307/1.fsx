#if INTERACTIVE
#else
namespace global
#endif

module Bindings = 

    open System.Windows
    open System.Windows.Data

    type DependencyPropertyBindingPair(dp:DependencyProperty,binding:Binding) =
        member this.Property = dp
        member this.Binding = binding
        static member (+) 
            (target:#FrameworkElement,pair:DependencyPropertyBindingPair) =
            target.SetBinding(pair.Property,pair.Binding) |> ignore
            target

    type DependencyPropertyValuePair(dp:DependencyProperty,value:obj) =
        member this.Property = dp
        member this.Value = value
        static member (+) 
            (target:#UIElement,pair:DependencyPropertyValuePair) =
            target.SetValue(pair.Property,pair.Value)
            target

    open System.Windows.Controls

    type Button with
        static member CommandBinding (binding:Binding) = 
            DependencyPropertyBindingPair(Button.CommandProperty,binding)
        static member EffectBinding (binding:Binding) =
            DependencyPropertyBindingPair(Button.EffectProperty,binding)

    type Grid with
        static member Column (value:int) =
            DependencyPropertyValuePair(Grid.ColumnProperty,value)
        static member Row (value:int) =
            DependencyPropertyValuePair(Grid.RowProperty,value)        

    type TextBox with
        static member TextBinding (binding:Binding) =
            DependencyPropertyBindingPair(TextBox.TextProperty,binding)

module ViewModel =

    type Operator =
        | Plus | Minus | Multiply | Divide
        static member Eval op (a:decimal) (b:decimal) =
            match op with
            | Plus -> a + b
            | Minus-> a - b
            | Multiply -> a * b
            | Divide -> a / b

    type Key =    
        | Digit of int
        | Dot
        | Operator of Operator
        | Evaluate

    let Command (action:'T -> unit) =
        let event = new DelegateEvent<System.EventHandler>()
        { new System.Windows.Input.ICommand with
            member this.Execute (param:obj) = action(param :?> 'T)
            member this.CanExecute (param:obj) = true
            [<CLIEvent>]
            member this.CanExecuteChanged = event.Publish
        }

    type Calculator() as this =   
        let propertyChanged = new Event<_,_>()
        let mutable display = 0M  
        let mutable acc = None    
        let mutable operation = None        
        let PropertyChanged name = 
            (this,System.ComponentModel.PropertyChangedEventArgs(name)) 
            |> propertyChanged.Trigger
        let SetOperation value =
            operation <- value
            PropertyChanged "Operator"
            PropertyChanged "Operand"
        let SetDisplay value =
            display <- value
            PropertyChanged "Display"        
        let Calculate () =
            match operation,acc with
            | Some(op,a), Some(b,_) -> 
                Operator.Eval op a b |> SetDisplay
            | _,_ -> ()
        let HandleKey = function
            | Digit(n) ->  
                let value,dp =       
                    match acc with
                    | Some(x,Some dp) ->                     
                        x + (dp * decimal n), Some(dp * 0.1M)                             
                    | Some(x,None) ->                                          
                        (x * 10M) + decimal n, None                    
                    | None -> decimal n, None
                acc <- Some(value,dp)
                value |> SetDisplay
            | Dot -> 
                acc <-
                    match acc with 
                    | Some(x,None) -> Some(x,Some(0.1M))
                    | Some(x,Some(dp)) -> acc
                    | None -> Some(0.0M,Some(0.1M))            
            | Operator(op) -> 
                Calculate ()
                SetOperation (Some(op,display))
                acc <- None     
            | Evaluate -> 
                Calculate ()
                SetOperation None
                acc <- None                      
        let command = Command(fun (param:Key) -> HandleKey (param))
        member this.KeyCommand = command                
        member this.Display = display 
        member this.Operator = operation |> Option.map fst
        member this.Operand = operation |> Option.map snd    
        interface System.ComponentModel.INotifyPropertyChanged with
            [<CLIEvent>]
            member this.PropertyChanged = propertyChanged.Publish

open Bindings
open ViewModel

open System
open System.Globalization
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Media
open System.Windows.Media.Effects

type CalculatorControl () as this =
    inherit UserControl (Width=240.0,Height=240.0) 

    let CreateTextBox (color,opactity) =
        TextBox(HorizontalContentAlignment=HorizontalAlignment.Right,
                Height=48.0,
                FontSize=32.0,
                Background=SolidColorBrush(color),
                Opacity=opactity,
                IsReadOnly=true)

    let display =
        let binding = 
            Binding("Display",Mode=BindingMode.OneWay,StringFormat="0.#####")    
        CreateTextBox (Colors.White,1.0) + TextBox.TextBinding(binding)
    
    let CreateValueConverter f =
        { new System.Windows.Data.IValueConverter with
            member this.Convert
                (value:obj,targetType:System.Type,parameter:obj,
                 culture:System.Globalization.CultureInfo) =
                f(value,parameter)
            member this.ConvertBack
                (value:obj,targetType:System.Type,parameter:obj,
                 culture:System.Globalization.CultureInfo) =
                raise (new System.NotImplementedException())
        } 

    let operationEffectConverter = 
        fun (value:obj,parameter:obj) -> 
            let op = value :?> Operator option
            let key = parameter :?> Key
            match op,key with
            | Some(op),Operator(x) when op = x -> DropShadowEffect() :> Effect
            | _ -> null :> Effect
            |> box
        |> CreateValueConverter     

    let keys =
        let grid = new Grid()
        for i = 1 to 4 do
            ColumnDefinition() |> grid.ColumnDefinitions.Add 
            RowDefinition() |> grid.RowDefinitions.Add
        [ 
        ['7',Digit(7);'8',Digit(8);'9',Digit(9);'/',Operator(Divide)]
        ['4',Digit(4);'5',Digit(5);'6',Digit(6);'*',Operator(Multiply)]
        ['1',Digit(1);'2',Digit(2);'3',Digit(3);'-',Operator(Minus)]
        ['0',Digit(0);'.',Dot;'=',Evaluate;'+',Operator(Plus)]
        ]    
        |> List.mapi (fun y ys ->
            ys |> List.mapi (fun x (c,key) ->
                let color =
                    match key with
                    | Operator(_) | Evaluate -> Colors.Yellow
                    | Digit(_) | Dot -> Colors.LightGray                        
                let effect =
                    Binding("Operator",
                            Converter=operationEffectConverter,
                            ConverterParameter=key)    
                Button(Content=c,CommandParameter=key,
                    Width=40.0,Height=40.0,Margin=Thickness(4.0),
                    Background=SolidColorBrush(color)) +                
                    Button.CommandBinding(Binding("KeyCommand")) +
                    Button.EffectBinding(effect) +
                    Grid.Column(x) + Grid.Row(y)                         
            )
        )
        |> List.concat
        |> List.iter (grid.Children.Add >> ignore)
        grid
           
    let (+.) (panel:#Panel) item = panel.Children.Add item |> ignore; panel                  
    let panel = StackPanel(Orientation=Orientation.Vertical) +. display +. keys        
    do this.Content <- panel
    let calculator = Calculator()           
    do this.DataContext <- calculator

#if INTERACTIVE
open Microsoft.TryFSharp
App.Dispatch (fun() -> 
    App.Console.ClearCanvas()
    CalculatorControl() |> App.Console.Canvas.Children.Add
    App.Console.CanvasPosition <- CanvasPosition.Right
)
#endif