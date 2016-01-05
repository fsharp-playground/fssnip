namespace FSharp.Converters

open System
open System.Windows
open System.Windows.Data
open Microsoft.FSharp.Reflection

[<AutoOpen>]
module FunctionLibrary = 
    let nullFunction = fun value _ _ _ -> value
    let stringToInt (a:Object) = Convert.ToInt32(a)
    let intToBool = fun i -> i = 0
    let boolToVisibility = fun b -> 
        if b then Visibility.Visible
        else Visibility.Collapsed

    let convert<'T> f (obj:System.Object) (t:Type) (para:System.Object) (culture:Globalization.CultureInfo)  = (obj :?> 'T) |> f |> box
    

/// abstract class for converter
[<AbstractClass>]
type ConverterBase(convertFunction, convertBackFunction) =    
    /// constructor take nullFunction as inputs
    new() = ConverterBase(nullFunction, nullFunction)

    // implement the IValueConverter
    interface IValueConverter with
        /// convert a value to new value
        override this.Convert(value, targetType, parameter, culture) =
            this.Convert value targetType parameter culture

        /// convert a value back
        override this.ConvertBack(value, targetType, parameter, culture) =
            this.ConvertBack value targetType parameter culture
    
    abstract member Convert : (obj -> Type -> obj -> Globalization.CultureInfo -> obj)
    default this.Convert = convertFunction

    abstract member ConvertBack : (obj -> Type -> obj -> Globalization.CultureInfo -> obj)
    default this.ConvertBack = convertBackFunction

/// Sample concrete implementation

type StringToVisiblityConverter() =
    inherit ConverterBase(stringToInt>>intToBool>>boolToVisibility |> convert, nullFunction)