open System.Management
/// Disconnect the PC from the internet.
/// I use this to reduce wasting time by stopping net surfing.
/// Offline environment is also useful for concentrate on coding.
/// This function requires admin rights.
let disconnectInternet _ =
 let wmiQuery = SelectQuery "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != null AND Manufacturer != 'Microsoft' "
 use searcher = new ManagementObjectSearcher(wmiQuery)
 for item in searcher.Get () do
   (item :?> ManagementObject).InvokeMethod("Disable", null) |> ignore