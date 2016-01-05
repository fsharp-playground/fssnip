#light

// Program to read a paper tape to a file from a GNT4604 paper tape reader
// attached to serial port COM3:

module ReadTape
    
    open System.IO

    exception NoTape  // used to force exit when no further tape to read

    // Open serial port 
    let OpenPort () =
        let port = new System.IO.Ports.SerialPort ("COM3", 4800, Ports.Parity.None, 8, Ports.StopBits.One)
        port.Open ()
        port.DtrEnable   <- true
        port.RtsEnable   <- true
        port.ReadTimeout <- 1000
        port 

    let port = OpenPort ()                   

    let ReadRaw f =
        let buffer: byte[] = Array.zeroCreate (250*60*30) // 15 minutes of paper tape reading 
        let rec GetBytes i = 
            try 
                    buffer.[i] <- byte (port.ReadByte ()) 
                    GetBytes (i+1)
            with 
            _   ->  i // finish on timeout    
        printfn "Reading tape to %s" f
        printf "Load tape and type hit any key..."
        System.Console.ReadLine () |> ignore
        let count = (GetBytes 30)+30 // force 3 inches of leader and trailer)
        printfn "%d bytes input" (count-60)
        let rec TrimLeft i =
            if   i = count
            then printfn "Empty tape"
                 0
            elif buffer.[i] = 0uy 
            then TrimLeft (i+1) 
            else i
        let rec TrimRight i =
            if   i = 0
            then printfn "Empty Tape"
                 count
            elif buffer.[i] = 0uy 
            then TrimRight (i-1) 
            else i
        let start  = max 30 ((TrimLeft 0)-30)
        let finish = min (count-30) ((TrimRight count)+30)       
        let trimmed = buffer.[start..finish]
        printfn "%d bytes output" trimmed.Length
        try File.WriteAllBytes (f, trimmed) with
        e   -> System.Console.WriteLine f


    let  cmdLine = System.Environment.GetCommandLineArgs()
    if   cmdLine.Length < 2
    then System.Console.WriteLine "Error: READTAPE file"
    else ReadRaw cmdLine.[1]
