/// strips values of reference types that are null
let strip<'T when 'T : null> (x : 'T) = match x with null -> None | x -> Some x

type DenullBuilder() =
    member __.Return x = x
    member __.Zero() = null
    member __.Bind(x, f) = match x with null -> null | x -> f x


let denull = new DenullBuilder()

// example
open Microsoft.Win32

denull {
    let bkey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
    let! k1 = bkey.OpenSubKey(@"Software\Microsoft\VisualStudio\11.0")
    let! k2 = k1.OpenSubKey(@"DialogPage\Microsoft.VisualStudio.FSharp.Interactive.FsiPropertyPage")
    let! switch = k2.GetValue "FsiPreferAnyCPUVersion" :?> string

    if switch = "True" then
        return k2.GetValue "FsiCommandLineArgs" :?> string
} |> strip