type AsyncSeq<'T> = Async<Chunk<'T>> 
and Chunk<'T> = 
    | Done
    | Value of 'T * AsyncSeq<'T>
 
module AsyncSeq =
    let map f (seq : AsyncSeq<_>) : AsyncSeq<_> = 
        let rec doMap s = async {
            let! chunk = s
            match chunk with
            | Done -> return Done
            | Value (v, next) -> return Value (f v, doMap next)
            }
        doMap seq
    
    let run action (seq : AsyncSeq<_>) = 
        let rec doRun s = async {
            let! chunk = s
            match chunk with
            | Done -> return ()
            | Value (v, next) -> action v; return! doRun next
            }
        doRun seq

    let filter f (seq : AsyncSeq<_>) : AsyncSeq<_> =
        let rec doFilter s = async { 
            let! chunk = s
            match chunk with
            | Value (value, next) -> 
                if f value then return Value(value, doFilter next)
                else return! doFilter next
            | x -> return x
            }
        doFilter seq

[<AutoOpen>]
module AsyncSeqExtensions = 
    
    open System.Text

    type System.IO.Stream with
        member this.AsyncReadSeq(?bufferSize) : AsyncSeq<byte[]> = 
            let bufferSize = defaultArg bufferSize (2 <<< 16)
            let temp : byte[] = Array.zeroCreate bufferSize
            let rec doRead () = async {
                let! count = this.AsyncRead(temp, 0, bufferSize)
                if count = 0 then return Done
                else 
                    let buf = Array.sub temp 0 count
                    return Value(buf, doRead ())
                }
            doRead ()
        
        member this.AsyncReadLines(?bufferSize) = 
            let sb = StringBuilder()
            let getText = AsyncSeq.map Encoding.UTF8.GetString

            let rec doRead (s : AsyncSeq<string>) = async {
                let! chunk = s
                match chunk with
                | Done -> 
                    if sb.Length <> 0 then return Value(sb.ToString(), async.Return Done )
                    else return Done
                | Value(part, next) -> 
                    return! doProcess part 0 next
                }
            and doProcess (text : string) n next = async {
                let (|Chars|) pos = 
                    if pos < text.Length - 1 then text.[pos], Some (text.[pos + 1])
                    else text.[pos], None
                
                let getLine newPos = 
                    let line = sb.ToString()
                    sb.Length <- 0
                    Some (line, newPos)

                let rec run n =
                    if n >= text.Length then None
                    else 
                        match n with
                        | Chars ('\r', Some '\n') -> getLine (n + 2)
                        | Chars ('\r', _) 
                        | Chars ('\n', _) -> getLine (n + 1)
                        | Chars (c, _) -> 
                            sb.Append(c) |> ignore
                            run(n + 1)
                match run n with
                | Some (line, pos) -> return Value (line, doProcess text pos next)
                | None -> return! doRead next
                }
                
            this.AsyncReadSeq(?bufferSize=bufferSize)
                |> AsyncSeq.map Encoding.UTF8.GetString
                |> doRead
        
        member this.AsyncWriteSeq(seq : AsyncSeq<byte[]>) = 
            let rec run s = async {
                let! chunk = s
                match chunk with
                | Done -> return ()
                | Value(data, next) ->
                    do! this.AsyncWrite(data)
                    return! run next
                }
            run seq       

open System.IO

let printWithPrefix path prefix = async {
    use f= File.Open(path, FileMode.Open)
    do! f.AsyncReadLines()
        |> AsyncSeq.map (sprintf "%s: %s" prefix)
        |> AsyncSeq.run (printfn "%s")
    }