module ConverterBase

open System;
open System.Windows
open System.Windows.Data

let nullFunction = fun value target param culture -> value

/// abstract class for converter
[<AbstractClass>]
type ConverterBase(convertFunction, convertBackFunction) =    
    /// constructor take nullFunction as inputs
    new() = ConverterBase(nullFunction, nullFunction)

    // implement the IValueConverter
    interface IValueConverter with
        /// convert a value to new vlaue
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
type UpperCaseConverter() =
    inherit ConverterBase()
    let convertFunc = fun (v:obj) _ _ _ -> (string v).ToUpper() :> obj
    override this.Convert = convertFunc 
