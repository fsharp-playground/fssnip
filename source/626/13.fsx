// Part of PortFusion (http://portfusion.sourceforge.net)
// Test at tryfs.net :)

#r "System.Management.dll"

module SystemID =

    open System
    open System.Management

    type ClassName     = string
    type PropertyKey   = string
    type PropertyValue = obj
    type IDComponent   = ClassName *  PropertyKey                 []
    type IDQueryResult = ClassName * (PropertyKey * PropertyValue)[][]

    let private prefix = "Win32_"                // !! Win32_* : CIM_*
    let inline  (!/) x = prefix + x

    let internal qs (t:ClassName) (ps:PropertyKey[]) =
        try use mos = new ManagementObjectSearcher("Select " + String.Join(",", ps) + " From " + t)
            let col = mos.Get()
            let arr = Seq.toArray
            seq { for o in col do
                    yield seq { for p in o.Properties do yield p.Name, p.Value } |> arr } |> arr
        with _ -> [|[||]|]
    let internal fs = Array.map (Array.filter (snd >> (<>) null))
    let internal gs (t, ps) = (t:ClassName).Substring prefix.Length, fs <| qs t ps

    let query : IDComponent[] -> IDQueryResult[] = Array.map gs

    module Conversions =
        let inline private bytes (raw:string) = System.Text.Encoding.UTF8.GetBytes raw
        let text rs =
            let b = System.Text.StringBuilder()
            rs |> Array.iter (sprintf "%A" >> b.AppendLine >> ignore)
            b.ToString()
        let sha (raw:string) =
            let sha = System.Security.Cryptography.SHA1.Create()
            raw |> bytes |> sha.ComputeHash |> Convert.ToBase64String
        let hex =  bytes >> BitConverter.ToString >> fun x -> x.Replace("-", "")


//
// Create custom system ID based on operating system serial number:
//

open SystemID
open SystemID.Conversions

let soft = [| !/"OperatingSystem", [| "SerialNumber" |] |] |> query

// val soft : IDQueryResult [] =
//   [|("OperatingSystem",
//      [|[|("SerialNumber", "92577-082-2500446-76172")|]|])|]

let softID = soft |> text |> sha |> hex

// val it : string = "723244466A616E484F6D4C4957795A6B6A78426141494C564E63513D"



//
// Create custom hardware ID based-on processor and BIOS information:
//

let hard = [| 

                !/"Processor", [| "Architecture"; "Caption"     ; "Name" |]
                !/"BIOS",      [| "ReleaseDate" ; "SerialNumber"         |]

           |] |> query

// val hard : IDQueryResult [] =
//   [|("Processor",
//      [|[|("Architecture", 9us);
//          ("Caption", "Intel64 Family 6 Model 30 Stepping 5");
//          ("Name", "Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz")|]|]);
//     ("BIOS", [|[|("ReleaseDate", "20090731000000.000000+000")|]|])|]

let hardID = hard |> text |> sha |> hex

// val it : string = "544E50614E4A3579784168496C6E4445683843684D6A72314F41493D"