open System
open System.Text
open System.IO

type OutStr(sb:StringBuilder, orig:TextWriter) =
    inherit TextWriter()
    override x.Encoding = stdout.Encoding
    override x.Write (s:string) = sb.Append s |> ignore; orig.Write s
    override x.WriteLine (s:string) = sb.AppendLine s |> ignore; orig.WriteLine s
    override x.WriteLine() = sb.AppendLine() |> ignore; orig.WriteLine()
    member x.Value with get() = sb.ToString()
    static member Create() =
        let orig = stdout
        let out = new OutStr(new StringBuilder(), orig)
        Console.SetOut(out)
        out
    interface IDisposable with member x.Dispose() = Console.SetOut(orig)

let withOutStr f a =
    use out = OutStr.Create()
    f(a), out.Value


(* Usage:

let f (a:int) =
    Console.WriteLine("I'm being evaluated to value {0}", a)
    a

let value,outstr = withOutStr f 2

printf "Output: %s" outstr

*)
