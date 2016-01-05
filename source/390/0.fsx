// [snippet:IO Monad]
namespace Haskell.Prelude

type IO<'T> = private | Action of (unit -> 'T)

[<AutoOpen>]
module MonadIO =
    let private mreturn x = Action (fun () -> x)
    let private run  io               = match io  with Action f -> f ()
    let private bind io  rest : IO<_> = match io  with Action f -> rest (f ())
    let private comb io1 (io2:IO<_>)  = match io1 with Action f -> f (); io2
    
    type IOBuilder() =
        member b.Run(io)           = run io
        member b.Return(x)         = mreturn x
        member b.ReturnFrom(io)    = io : IO<_>
        member b.Delay(g)          = mreturn (run (g ()))
        member b.Bind(io, rest)    = bind io rest
        member b.Combine(io1, io2) = comb io1 io2
    
    let io = new IOBuilder()
    let (|Action|) io = run io

[<AutoOpen>]
module PreludeIO =
    let putChar  (c:char)   = Action (fun () -> stdout.Write(c))
    let putStr   (s:string) = Action (fun () -> stdout.Write(s))
    let putStrLn (s:string) = Action (fun () -> stdout.WriteLine(s))
    let print x             = Action (fun () -> printfn "%A" x)
    let getChar     = Action (fun () -> stdin.Read() |> char |> string)
    let getLine     = Action (fun () -> stdin.ReadLine())
    let getContents = Action (fun () -> stdin.ReadToEnd())
// [/snippet]

// [snippet:Usage]
namespace HaskellStyleIO

open System
open Haskell.Prelude

module Program =
    let lines (s:string) = s.Split([|stdout.NewLine|], StringSplitOptions.None) |> Seq.ofArray
    let length sq = Seq.length sq
    
    [<EntryPoint>]
    let main _ =
        // get/put two lines
        let (Action ()) = io {
            let! cs1 = getLine
            let! cs2 = getLine
            return! putStrLn cs1
            return  putStrLn cs2
        }
        // cat
        let (Action ()) = io {
            let! cs = getContents
            return putStr cs
        }
        // wc -l
        let (Action ()) = io {
            let! cs = getContents
            return cs |> lines |> length |> print
        }
        0
// [/snippet]