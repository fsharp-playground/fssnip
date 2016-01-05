    open System
    open System.Reflection
    open System.Windows
    open System.Windows.Controls
    open System.Windows.Markup

    type RoomsListView() as this =
        inherit UserControl()

        do
            let sr = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoomsListView.xaml")
            let test = XamlReader.Load(sr) :?> UserControl
            this.Content <- test.Content

        let mutable layoutRoot : Grid = downcast this.FindName("LayoutRoot")

        do
            ()