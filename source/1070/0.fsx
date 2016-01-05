open System.Net
open System.Text 
open System.Net.Sockets

module stream =
  type stream               = NetworkStream
  let curry g b n           = g(b,0,n) |> ignore; b
  let read  (s : stream) n  = curry s.Read (Array.zeroCreate n) n
  let write (s : stream) b  = curry s.Write b b.Length; s
  let close (s : stream) b  = s.Close(); b
  let connect host port     = TcpClient(host,port).GetStream()

(* example *) 
let sendString host (data : string) =
  stream.connect host 80 
  |> fun stm -> 
     data
     |> Encoding.Default.GetBytes 
     |> stream.write stm 
     |> stream.read <| 256
     |> Encoding.Default.GetString
     |> stream.close stm

let data = "GET / HTTP/1.1\r\nHost: microsoft.com\r\n\r\n"

// not robust for all server types, just for demo, no exceptions.
let getServerHeader (s : string) =
  let needle = s.IndexOf "Server: "
  s.[needle..needle+24]

sendString "microsoft.com" data |> getServerHeader

(* val it : string = "Server: Microsoft-IIS/7.5" *)