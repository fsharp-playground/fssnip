    let inline (|&) x a = x |> a |> ignore; x

    let inline fac<'T>() = FrameworkElementFactory(typeof<'T>)    
    let inline (<>=) o p v = ( ^a : (member SetValue   : DependencyProperty -> obj              -> unit) (o, p, v) )
    let inline (<>~) o p b = ( ^a : (member SetBinding : DependencyProperty -> Data.BindingBase -> unit) (o, p, Data.Binding b) )
    let inline (<>+) o v   = ( ^a : (member AppendChild : 'b -> unit) (o, v) )
    let inline (<>~<) o p (b,c) =
        ( ^a : (member SetBinding : DependencyProperty -> Data.BindingBase -> unit) (o, p, Data.Binding b |& fun x -> x.Converter <- c) )
    let makeValueConverter (f,g) =
        {
            new Data.IValueConverter with
                member this.Convert    (v, tt, p, c) = f v
                member this.ConvertBack(v, tt, p, c) = g v
        }
    let inline (>=<) (f : 'a -> 'b) (g : 'b -> 'a) = makeValueConverter (unbox >> f >> box,unbox >> g >> box)
    let inline (<>++) o vs = ((vs |> Seq.toArray) |>|! (<>+) o) |> ignore
    let inline (<>|+) o e h = ( ^a : (member AddHandler : RoutedEvent -> 'd -> unit) (o, e, h) )


//////////////////////////// USAGE


    let itemTemplate =

        let c2 = CORSIS.PDF.Windows.Documents.Overlays.reasonColor >> brush >=< fun _ -> ""

        DataTemplate() |& fun dt ->
            dt.DataType   <- typeof<Bookmark Choice>
            dt.VisualTree <- fac<StackPanel>() |& fun sp ->
                sp <>= StackPanel.OrientationProperty <| Orientation.Horizontal
                sp <>++
                    [
                        fac<TextBlock>() |& fun tb ->
                            tb <>~< TextBlock.TextProperty <| ("Value.PageIndex", (+) 1 >=< (-) 1)
                            tb <>=  TextBlock.FontFamilyProperty <| FontFamily "Consolas"
                            tb <>=  TextBlock.ForegroundProperty <| Brushes.LightGray
                        fac<TextBlock>() |& fun tb ->
                            tb <>=  TextBlock.TextProperty <| " "
                            tb <>~< TextBlock.BackgroundProperty <| ("Value.Reason", c2)
                            tb <>=  TextBlock.MarginProperty <| Thickness(4., 1., 4., 1.)
                        fac<TextBlock>() |& fun tb -> tb <>~  TextBlock.TextProperty <| "Value.Text"
                    ]
