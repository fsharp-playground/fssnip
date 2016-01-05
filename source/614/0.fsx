(* Windows Internals 5th Edition, P48 *)
#r "System.Management.dll";;
open System.Management
open System.Collections.Generic

let getObj (scope : string) query = 
  ManagementObjectSearcher(scope, query).Get()

getObj @"\root\cimv2" "SELECT * FROM Win32_OperatingSystem" 
|> Seq.cast 
|> Seq.map(fun (x : ManagementObject) -> 
  [x.["Caption"]
   x.["Debug"]
   x.["Version"] ])
;;

