
#nowarn "62"

open System
open Microsoft.Win32

let workingDir = "E:\\win_reg\\"

type regQuery = {
  name           : string
  defaultVal     : string
  recommendedVal : string
  registryDir    : string
  registryKey    : string
}

// we keep output as obj because we printf it with `%A' 
// this means we dont have to do key specific conversions!
type csvOutput = {
  input  : regQuery
  output : obj }

let csvToRegQuery (xs : string) =
  let s = xs.Split ','
  { name           = s.[1]
    defaultVal     = s.[2]
    recommendedVal = s.[3]
    registryDir    = s.[4]
    registryKey    = s.[5] 
  }

let parseInputCsv = 
  IO.File.ReadAllLines >> Seq.map csvToRegQuery

let getValue (q : regQuery) =
  { input  =  q
    output =  Registry.GetValue(q.registryDir, q.registryKey, "")
  }

let writeCsvOutput (data : csvOutput seq) =
  let hdr = ",name,default,recommended,current,reg,key,\r\n"
  let fmt = sprintf ",%s,%s,%s,%A,%s,%s,\r\n"

  let append data = 
    IO.File.AppendAllText(workingDir ^ "out.csv",data)

  append hdr
  data |> Seq.map (fun x -> 
    fmt x.input.name 
        x.input.defaultVal 
        x.input.recommendedVal 
        x.output 
        x.input.registryDir 
        x.input.registryKey)
      |> Seq.iter append

workingDir ^ "Windows-All-Reg.csv"
|> parseInputCsv 
|> Seq.map getValue 
|> writeCsvOutput

(* Example of file Windows-All-Reg.csv is:
,name,default,recommended,reg,key,
,Disable anonymous enum of SAM accounts and shares,0,1,HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa,RestrictAnonymous,
,Disable storage of creds for network auth,0,1,HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa,DisableDomainCreds,

and the output:
,name,default,recommended,current,reg,key,
,Disable anonymous enum of SAM accounts and shares,0,1,0,HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa,RestrictAnonymous,
,Disable storage of creds for network auth,0,1,0,HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa,DisableDomainCreds,
*)