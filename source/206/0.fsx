// Consider we have a class "WaterMarkTextBox" and it requires
// a WaterMarkText property.
// Declare the static field for DependencyProperty    
static let WaterMarkTextProperty = DependencyProperty.Register("WaterMarkText", typeof<string>, 
typeof<WaterMarkTextBox>, new PropertyMetadata(String.Empty))

// Gets / Sets the WaterMarkText
member x.WaterMarkText
    with get() = x.GetValue(WaterMarkTextProperty) :?> string
    and set(v) = x.SetValue(WaterMarkTextProperty, v)

// Consider we have a PropertyMetadata for an ItemsSource property 
// with a callback
// Below is the code for ItemsSource Dependency Property
static let itemsSourceMetadata = 
        new PropertyMetadata
            ( null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                        let box = dpo :?> SearchBox
                        if args.NewValue <> null then
                            box.OnItemsSourceChanged(args.NewValue :?> IEnumerable)
                    )
                )                 
            )
static let ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof<IEnumerable>, 
typeof<SearchBox>, itemsSourceMetadata)

member private x.OnItemsSourceChanged(itemsSource : IEnumerable) =
   (*[omit:(...)]*)

// Gets / Sets the ItemsSource
member x.ItemsSource 
   with get() = x.GetValue(ItemsSourceProperty) :?> IEnumerable
   and set(v) = x.SetValue(ItemsSourceProperty, v)