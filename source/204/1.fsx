open System
open System.Collections
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Windows
open System.Windows.Data
open System.Windows.Controls
open System.Windows.Documents
open System.ComponentModel

// Consider that you have a generic.xaml inside "Themes" 
// folder. For more details on this check this post:
// http://fadsworld.wordpress.com/2011/03/05/f-wpf-component-development/

type CustomControl1() as this =
    inherit Control()

    do this.DefaultStyleKey <- typeof<CustomControl1>
    let path = "/FSharpWpfCustomControlLibrary1;" +
               "component/Themes/generic.xaml"
    let uri = new Uri(path, System.UriKind.Relative)
    let resourceDict = Application.LoadComponent(uri) :?> ResourceDictionary          
    do this.Resources.MergedDictionaries.Add(resourceDict)

    override this.OnApplyTemplate() =
        printfn "template applied"