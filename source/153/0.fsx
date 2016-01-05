open System
open System.Windows.Forms
open System.Net
open System.Net.Sockets
open System.Text
open System.IO

type Stream with
    member this.WriteString(str:string) = 
        let bytes = Encoding.UTF8.GetBytes(str)
        this.Write(bytes,0,bytes.Length)
        this.Flush()


let filterHttpHeader (input:seq<char>) =   
    let rec filter a b c d = 
        if a = '\r' && b = '\n' && c = '\r' && d = '\n' then
            input
        else
            filter b c d (Seq.head input)
    filter (Seq.head input) (Seq.head input) (Seq.head input) (Seq.head input)

let getChunks (input:seq<char>) = 
    seq {
        while true do
            let len = 
                input
                |> Seq.takeWhile (fun c -> c <> '\n')
                |> Seq.fold (fun (s:StringBuilder) c -> s.Append(c)) (new StringBuilder())
                |> (fun s -> s.ToString().Replace("\r",""))
                |> (fun s -> Int32.Parse(s,Globalization.NumberStyles.HexNumber))
            let str = input
                        |> Seq.take (len+2)
                        |> Seq.fold (fun (s:StringBuilder) c -> s.Append(c)) (new StringBuilder())
            yield str.ToString()
    }

let cl = new TcpClient()
cl.Connect("localhost",80)
let out = cl.GetStream()
out.WriteString ("GET /channel/ HTTP/1.1\r\nContent-Type: text/plain\r\nHost: localhost\r\nTransfer-Encoding: chunked\r\n\r\n")
async {
    seq {
        while true do
            yield out.ReadByte()
    }
    |> Seq.map (fun b -> (char b))
    |> filterHttpHeader
    |> getChunks
    |> Seq.iter (fun s -> Console.WriteLine s)
    ()
} |> Async.Start 