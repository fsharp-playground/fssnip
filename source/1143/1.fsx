    open System
    open System.Reflection
    open System.Windows
    open System.Windows.Controls
    open System.Windows.Markup

    type RoomsListView() as this =
        inherit UserControl()

        do
            let sr = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoomsListView.xaml")
            this.Content <- XamlReader.Load(sr) :?> UserControl

        let mutable layoutRoot : Grid = downcast this.FindName("LayoutRoot")

        do
            ()