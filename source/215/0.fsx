open System.IO
open System.Threading
open System.Net
open System.Net.Sockets

let mirror (clientStream:Stream) (serverStream:Stream) = async {
    while true do
        let! onebyte = clientStream.AsyncRead(1)
        do! serverStream.AsyncWrite(onebyte) 
}

let proxy (clientStream:Stream) (serverStream:Stream) = 
    [| mirror clientStream serverStream; mirror serverStream clientStream |]
        |> Async.Parallel
        |> Async.RunSynchronously 
        
let stream (client:TcpClient) = client.GetStream()

let tcp_ip_proxy (sourceip,sourceport) (targetip,targetport) = 
    let server = new TcpListener(IPAddress.Parse(sourceip),sourceport)
    server.Start()
    while true do 
        let client = server.AcceptTcpClient()
        let up = new TcpClient(targetip,targetport)
        let t = new Thread(ThreadStart(fun _ -> 
            try      
                stream (client) |> proxy <| stream(up) |> ignore 
            with |_ -> ())
        , IsBackground = true)
        t.Start()   

tcp_ip_proxy ("127.0.0.1",8080) ("66.249.81.104",80)