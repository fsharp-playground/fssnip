#nowarn "51"

module private Interop =
    open System
    open System.Runtime.InteropServices

    /// SYSTEMTIME structure used by SetLocalTime
    [<Struct>]
    type SYSTEMTIME =
        val year: int16
        val month: int16
        val dayOfWeek: int16
        val day: int16
        val hour: int16
        val minute: int16
        val second: int16
        val ms: int16
        new (time:DateTime) = {
            year      = int16 time.Year
            month     = int16 time.Month
            dayOfWeek = int16 time.DayOfWeek
            day       = int16 time.Day
            hour      = int16 time.Hour
            minute    = int16 time.Minute
            second    = int16 time.Second
            ms        = int16 time.Millisecond
        }
 
    [<DllImport("kernel32.dll")>]
    extern void SetLocalTime(SYSTEMTIME* t)

    let setSystemTime (time:DateTime) =
        let mutable st = SYSTEMTIME(time)
        SetLocalTime(&&st)

module NtpClient =

    // based on code from http://www.codeproject.com/KB/IP/ntpclient.aspx

    open System
    open System.Net
    open System.Net.Sockets

    type System.Net.Sockets.UdpClient with
        member x.AsyncSend (bytes: byte[]) =
            Async.FromBeginEnd((fun (ar, s) -> x.BeginSend(bytes, bytes.Length, ar, s)), x.EndSend)

        member x.AsyncReceive (endPoint: IPEndPoint ref) =
            Async.FromBeginEnd(x.BeginReceive, fun ar -> x.EndReceive(ar, endPoint))

    /// Leap year indicator
    type LeapIndicator =
        /// 0 - No warning
        | NoWarning
        /// 1 - Last minute has 61 seconds
        | LastMinute61
        /// 2 - Last minute has 59 seconds
        | LastMinute59
        /// 3 - Alarm condition (clock not synchronized)
        | Alarm

    type Mode =
        | SymmetricActive
        | SymmetricPassive
        | Client
        | Server
        | Broadcast
        | Unknown

    type Stratum =
        | Unspecified
        | PrimaryReference
        | SecondaryReference
        | Reserved

    // NTP data structure length
    let [<Literal>] private ntpDataLength = 48
    // offset constants for timestamps in the data structure
    let [<Literal>] offRefID  = 12
    let [<Literal>] offRefTS  = 16
    let [<Literal>] offOrigTS = 24
    let [<Literal>] offRcvTS  = 32
    let [<Literal>] offTrnTS  = 40
    let epoch = DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)

    /// Initialize a data array with a client request and the current timestamp.
    let initData () =
        let ms = uint64 (DateTime.UtcNow - epoch).TotalMilliseconds
        let intpart = ms / 1000UL
        let fractpart = (((ms % 1000UL) * 0x100000000UL) / 1000UL)
        Array.init ntpDataLength
            (function
             | 0 -> 0x1Buy // Set version number to 4 and Mode to 3 (client)
             | x when x = offTrnTS + 0 -> byte (intpart >>> 24)
             | x when x = offTrnTS + 1 -> byte (intpart >>> 16)
             | x when x = offTrnTS + 2 -> byte (intpart >>> 08)
             | x when x = offTrnTS + 3 -> byte intpart
             | x when x = offTrnTS + 4 -> byte (fractpart >>> 24)
             | x when x = offTrnTS + 5 -> byte (fractpart >>> 16)
             | x when x = offTrnTS + 6 -> byte (fractpart >>> 08)
             | x when x = offTrnTS + 7 -> byte fractpart
             | _ -> 0uy)


    type NtpInfo(data: byte[]) =

        let creationTime = DateTime.Now
        
        /// Compute the date, given the offset of a 8-byte array
        let computeDate offset =
            let intpart =
                (uint64 data.[offset + 0] <<< 24) |||
                (uint64 data.[offset + 1] <<< 16) |||
                (uint64 data.[offset + 2] <<< 08) |||
                (uint64 data.[offset + 3])
            let fractpart =
                (uint64 data.[offset + 4] <<< 24) |||
                (uint64 data.[offset + 5] <<< 16) |||
                (uint64 data.[offset + 6] <<< 08) |||
                (uint64 data.[offset + 7])
            let ms = intpart * 1000UL + (fractpart * 1000UL) / 0x100000000UL
            let elapsed = TimeSpan.FromMilliseconds(float ms)
            epoch + elapsed
    
        member x.LeapIndicator =
            // Isolate the two most significant bits
            match data.[0] >>> 6 with
            | 0uy -> NoWarning
            | 1uy -> LastMinute61
            | 2uy -> LastMinute59
            | _   -> Alarm

        member x.VersionNumber =
            // isolate bits 3-5
            (data.[0] &&& 0x38uy) >>> 3

        member x.Mode =
            // Isolate bits 0-3
            match data.[0] &&& 0x7uy with
            | 1uy -> SymmetricActive
            | 2uy -> SymmetricPassive
            | 3uy -> Client
            | 4uy -> Server
            | 5uy -> Broadcast
            | _   -> Unknown

        member x.Stratum =
            match data.[1] with
            | 0uy              -> Unspecified
            | 1uy              -> PrimaryReference
            | x when x <= 15uy -> SecondaryReference
            | _                -> Reserved

        member x.PollInterval =
            pown (uint32 data.[2]) 2

        /// Precision (in milliseconds)
        member x.Precision =
            (pown (uint32 data.[3]) 2) * 1000u

        /// Root Delay (in milliseconds)
        member x.RootDelay =
            let temp =
                uint32 data.[4] <<< 24 |||
                uint32 data.[5] <<< 16 |||
                uint32 data.[6] <<< 08 |||
                uint32 data.[7]
            1000.0 * (float temp / 0x10000LF)

        /// Root Dispersion (in milliseconds)
        member x.RootDispersion =
            let temp =
                uint32 data.[8]  <<< 24 |||
                uint32 data.[9]  <<< 16 |||
                uint32 data.[10] <<< 08 |||
                uint32 data.[11]
            1000.0 * (float temp / 0x10000LF)

        member x.ReferenceId =
            match x.Stratum with
            | PrimaryReference | Unspecified ->
                String([| char data.[offRefID + 0]
                          char data.[offRefID + 1]
                          char data.[offRefID + 2]
                          char data.[offRefID + 3] |])
            | SecondaryReference ->
                match x.VersionNumber with
                | 3uy -> // Version 3, Reference ID is an IPv4 address
                    let address = 
                        IPAddress([| data.[offRefID + 0]
                                     data.[offRefID + 1]
                                     data.[offRefID + 2]
                                     data.[offRefID + 3] |])
                    try
                        let host = Dns.GetHostEntry(address)
                        sprintf "%s (%O)" host.HostName address
                    with _ ->
                        address.ToString()
                | 4uy -> // Version 4, Reference ID is the timestamp of last update
                    let time = computeDate offRefID
                    // take care of time zone
                    let offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
                    (time + offset).ToString()
                | _ -> "N/A"
            | Reserved -> "N/A"

        member x.ReferenceTimestamp =
            let time = computeDate offRefTS
            let offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
            time + offset

        member x.OriginateTimestamp =
            let time = computeDate offOrigTS
            let offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
            time + offset

        member x.ReceiveTimestamp =
            let time = computeDate offRcvTS
            let offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
            time + offset

        member x.TransmitTimestamp =
            let time = computeDate offTrnTS
            let offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
            time + offset

        member x.ReceptionTimestamp =
            creationTime

        /// Round trip delay (in milliseconds)
        member x.RoundTripDelay =
            let delay = (x.ReceiveTimestamp - x.OriginateTimestamp) + (x.ReceptionTimestamp - x.TransmitTimestamp)
            int delay.TotalMilliseconds

        /// Local clock offset (in milliseconds)
        member x.LocalClockOffset =
            let offset = (x.ReceiveTimestamp - x.OriginateTimestamp) - (x.ReceptionTimestamp - x.TransmitTimestamp)
            int (offset.TotalMilliseconds / 2.0)

        override x.ToString() =
            sprintf "%A" x

    
    let query server = 
        async {
            let! hosts = Async.FromBeginEnd(server, Dns.BeginGetHostAddresses, Dns.EndGetHostAddresses)
            if hosts = null || hosts.Length = 0 then
                failwithf "Could not resolve IP address for host '%s'." server

            let endPoint = IPEndPoint(hosts.[0], 123)
            use timeSocket = new UdpClient()
            timeSocket.Connect endPoint
            let sentData = initData()
            let! sentBytes = timeSocket.AsyncSend(sentData)
            let! rcvData = timeSocket.AsyncReceive(ref endPoint)
            timeSocket.Close()
            let info = NtpInfo(rcvData)
            if rcvData.Length <> ntpDataLength || info.Mode <> Server then
                failwithf "Invalid response from %s." server

            return info
        }

    let queryAsTask =
        query >> Async.StartAsTask
        
    let updateClock server =
        async {
            let! timeInfo = query server
            // add half the round-trip delay to the server time
            let targetTime = timeInfo.TransmitTimestamp + TimeSpan.FromMilliseconds(float timeInfo.RoundTripDelay / 2.0)
            Interop.setSystemTime targetTime
            printfn "Set local time to %A." targetTime
        } |> Async.RunSynchronously
