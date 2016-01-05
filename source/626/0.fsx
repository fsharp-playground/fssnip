#r "System.Management.dll"

module SystemID =

    open System
    open System.Management

    type ClassName     = string
    type PropertyKey   = string
    type PropertyValue = obj
    type IDComponent   = ClassName *  PropertyKey                 []
    type IDQueryResult = ClassName * (PropertyKey * PropertyValue)[][]

    let internal qs (t:ClassName) (ps:PropertyKey[]) =
        try use mos = new ManagementObjectSearcher("Select " + String.Join(",", ps) + " From " + t)
            let col = mos.Get()
            let arr = Seq.toArray
            seq { for o in col do
                    yield seq { for p in o.Properties do yield p.Name, p.Value } |> arr } |> arr
        with _ -> [|[||]|]
    let internal gs s = s|>Array.map (Array.filter (fun e -> match e with k,null -> false |_->true))
    let internal vs (t, ns) = t,qs t ns |> gs

    let query : IDComponent[] -> IDQueryResult[] = fun (cs:IDComponent[]) -> cs |> Array.map vs

    module Conversions =
        let inline private bytes (raw:string) = System.Text.Encoding.UTF8.GetBytes raw
        let text rs =
            let b = System.Text.StringBuilder()
            rs |> Array.iter (sprintf "%A" >> b.AppendLine >> ignore)
            b.ToString()
        let sha (raw:string) =
            let sha = System.Security.Cryptography.SHA1.Create()
            raw |> bytes |> sha.ComputeHash |> System.Convert.ToBase64String
        let inline hex raw = raw |> bytes |> System.BitConverter.ToString |> fun x -> x.Replace("-", "")

    let inline (!/) x = "Win32_" + x


//
// Create custom system ID based on operating system serial number:
//

open SystemID
open SystemID.Conversions

let soft = [| !/"OperatingSystem", [| "SerialNumber" |] |] |> query

// val soft : IDQueryResult [] =
//   [|("Win32_OperatingSystem",
//      [|[|("SerialNumber", "########################")|]|])|]

soft |> text |> sha |> hex

// val it : string = "433855487A6D4F76453845736667777061724A6E5336734F4434513D"


//
// Create custom hardware ID based-on processor and BIOS information:
//

let hard = [| 

                !/"Processor", [| "Architecture"; "Caption"     ; "Name" |]
                !/"BIOS",      [| "ReleaseDate" ; "SerialNumber"         |]

           |] |> query

// val hard : IDQueryResult [] =
//   [|("Win32_Processor",
//      [|[|("Architecture", 9us);
//          ("Caption", "Intel64 Family 6 Model 30 Stepping 5");
//          ("Name", "Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz")|]|]);
//     ("Win32_BIOS", [|[|("ReleaseDate", "20090731000000.000000+000")|]|])|]

hard |> text |> sha |> hex

// val it : string = "544E50614E4A3579784168496C6E4445683843684D6A72314F41493D"