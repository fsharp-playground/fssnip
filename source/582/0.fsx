(* Vortext level0 OTW*)
open System
open System.Net.Sockets

let server, port = "vortex.labs.overthewire.org", 5842

(* tried to remove new but we get annoying useless warnings *)
let s = (new TcpClient(server, port)).GetStream()

let recvData recvLen =
  let buff = Array.zeroCreate recvLen
  s.Read(buff, 0, buff.Length)
    
let sendData data = 
  s.Write(data, 0, data.Length)  
  printfn "%A" (Text.Encoding.Default.GetString(recvData 40))
  
[for i in 0..3 do yield recvData 4 ]
  |> List.map(fun e -> BitConverter.ToInt32(e,0) ) 
  |> List.fold (+) 0
  |> BitConverter.GetBytes
  |> sendData