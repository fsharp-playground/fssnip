open System
open System.Net.Sockets
open System.Text

type RedisClient () =

    // Socket
    let clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    do clientSocket.Connect("127.0.0.1", 6379)
    let netStream = new NetworkStream(clientSocket, true)

    // Helper
    let toS (o:obj) = Convert.ToString(o)
    let toC (o:obj) = Convert.ToChar(o)
    let getBytes (s:string) = Encoding.UTF8.GetBytes(s) 
    let getString (b:byte[]) = Encoding.UTF8.GetString(b)                                                               
    let mergeCommand (l:list<String>) = 
            let sb = new StringBuilder()
            sb.Append("*" + toS l.Length + "\r\n") |> ignore
            l |> List.iter (fun item -> sb.Append("$" + toS item.Length + "\r\n") |> ignore
                                        sb.Append(toS item + "\r\n") |> ignore )
            getBytes <| sb.ToString()            

    // Member    
    member x.AsyncSend l = async { let b = mergeCommand l
                                   do! netStream.AsyncWrite(b, 0, b.Length) }

    member x.SyncSend l = let b = mergeCommand l
                          netStream.Write(b, 0, b.Length)

    member x.ReadLine () = let n = ref 0
                           let sb = new StringBuilder()        
                           while !n <> 10 do
                             n := netStream.ReadByte()
                             if !n <> 13 && !n <> 10 then sb.Append(toC !n) |> ignore
                           sb.ToString()
                                                    
    member x.Close () = netStream.Close()

             
// Test Helper
let values m = List.concat [for i in 1 .. m -> ["K" + i.ToString(); "V" + i.ToString()] ]
               // ["K1";"V1";"K2";"V2";"K3";"V3"; ... ]

let SyncTest n v =  [ for i in 1 .. n ->
                        async {
                            let r = new RedisClient()
                            r.SyncSend("MSET" :: v)
                            let s = r.ReadLine()
                            r.Close()
                            return s                }] |> Async.Parallel |> Async.RunSynchronously

let AsyncTest n v =  [ for i in 1 .. n ->
                        async {
                            let r = new RedisClient()
                            do! r.AsyncSend("MSET" :: v)
                            let s = r.ReadLine()
                            r.Close() 
                            return s                }] |> Async.Parallel |> Async.RunSynchronously

let RealSync n v =  [ for i in 1 .. n ->
                        let r = new RedisClient()
                        r.SyncSend("MSET" :: v)
                        let s = r.ReadLine()
                        r.Close() 
                        s                       ] 
// Test
#time
let shortValues = values 100
let longValues  = values 2000
     
SyncTest  3000  shortValues 
AsyncTest 3000  shortValues 
RealSync  3000  shortValues 
 
SyncTest  3000  longValues 
AsyncTest 3000  longValues // Async version will stop respond when data are too large.
                           // Don't know how to solve yet.
RealSync  3000  longValues
