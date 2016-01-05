open System
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open System.Windows

[<STAThread>]
[<EntryPoint>]
let main argv = 
    Clipboard.Clear()
    printfn "Hit more than 100 keys randomly."

    let s = new StringBuilder(512)
    let input = ref (Console.ReadKey(true))
    while
        (s.Length < 100)
        || not (input.contents.Key.Equals(ConsoleKey.Enter))
        do
            input := Console.ReadKey(true)
            if not (input.contents.Key.Equals(ConsoleKey.Enter))
            then s.Append(input.contents.Key) |> ignore
            else if s.Length < 100 then printfn "Need more %d keys." (100 - s.Length)

    let sha512 = SHA512.Create()
    let hash = sha512.ComputeHash(Encoding.ASCII.GetBytes(s.ToString()))
    let b64 = Convert.ToBase64String(hash)
    let excludeCharsPattern = @"[lIO0\+\/=\n]"
    let generated = Regex.Replace(b64, excludeCharsPattern, "")
    Clipboard.SetText(generated)
    printfn "Password-like text is saved to clipboard."
    0
