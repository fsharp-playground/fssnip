/// strips values of reference types that are null
let denull<'T when 'T : null> (x : 'T) =
    match x with null -> None | x -> Some x

// define the option monad
type OptionBuilder() =
    member __.Return x = Some x
    member __.Bind(x,f) = Option.bind f x
    member __.ReturnFrom (x : 'T option) = x

let option = new OptionBuilder()

// example
open Microsoft.Win32

option {
    let bkey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
    let! k1 = denull <| bkey.OpenSubKey(@"Software\Microsoft\VisualStudio\11.0")
    let! k2 = denull <| k1.OpenSubKey(@"DialogPage\Microsoft.VisualStudio.FSharp.Interactive.FsiPropertyPage")
    let! switch = denull (k2.GetValue "FsiPreferAnyCPUVersion" :?> string)

    if switch = "False" then
        return! denull (k2.GetValue "FsiCommandLineArgs" :?> string)
    else
        return! None
}