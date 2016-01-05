// Some F# core types have values representable with nulls.
let usesNullAsTrueValue<'T> =
    if typeof<'T> = typeof<unit> then true
    else
        let attrs = typeof<'T>.GetCustomAttributes(typeof<CompilationRepresentationAttribute>, false)
        if attrs.Length = 0 then false
        else
            let flags = (attrs.[0] :?> CompilationRepresentationAttribute).Flags
            flags.HasFlag CompilationRepresentationFlags.UseNullAsTrueValue

let stripNull<'T when 'T : not struct> (x : 'T) =
    if obj.ReferenceEquals(x, null) && not usesNullAsTrueValue<'T> then None
    else Some x

// model the builder after the Option monad; could do otherwise I guess
type DenullBuilder() =
    member __.Return(x : 'T) = stripNull x
    member __.Bind(x : 'T, f : 'T -> 'S option) = Option.bind f (stripNull x)
    member __.ReturnFrom(x : 'T option) = x

let denull = new DenullBuilder()

// example
open Microsoft.Win32

denull {
    let bkey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
    let! k1 = bkey.OpenSubKey(@"Software\Microsoft\VisualStudio\11.0")
    let! k2 = k1.OpenSubKey(@"DialogPage\Microsoft.VisualStudio.FSharp.Interactive.FsiPropertyPage")
    let! switch = k2.GetValue("FsiPreferAnyCPUVersion") :?> string

    if switch = "False" then
        return k2.GetValue("FsiCommandLineArgs") :?> string
    else
        return! None
}