// rference to the dll from KpNet to build connection to kdb.
#r @"C:\q\kpnet\release\KpNet.KdbPlusClient.dll"

open System
open System.Net
open System.Text.RegularExpressions

let matcher input  = Regex.Matches(input ,"\"/q\?s=(.*?)\"")

let webClient = new WebClient()

let numperpage = 51
let indexurl sym num= "http://finance.yahoo.com/q/cp?s="+sym+"&c="+num.ToString()
// lets download all the constituents from these index
let rawData = [("^GSPC",500); ("^FTSE",100); ("^HSI",45);("^STOXX50E",50);("^DJI",30)]

let rawSym = rawData |> List.fold(
                            fun list (index,num) -> 
                                let nnn = num / numperpage
                                let nn = nnn-1
                                let n = max nn 0
                                let pages = seq{ for i in [0 .. n ]  -> indexurl index i} |> Seq.toList
                                let result = pages |> Seq.fold( fun listToAppend url -> 
                                                        let matches = matcher(webClient.DownloadString(url))
                                                        let r = [for m in matches -> (m.Groups.Item 1).ToString()] |> List.tail
                                                        r |> List.append listToAppend
                                              ) List.empty<string>
                                (index,result)::list
                        ) List.empty<string*string list>

//change the parameters according to your settings.
let kdb = KpNet.KdbPlusClient.KdbPlusDatabaseClient.Factory.CreateNonPooledClient("server=localhost;port=1444")

// we save the constituents into a kdb
// needs to be extended with date, because the constiuents might changed.
let constituens = "constituens"
kdb.ExecuteNonQuery(constituens+":()!()" );
rawSym |> List.iter(fun (index,ll) ->
                         let data = constituens+"[`$"+"\""+index+"\""+"]"
                         ll  |> List.iter( fun c -> kdb.ExecuteNonQuery(data+":"+data+",enlist `$\""+c+"\"" ) ) 

                   )

let table = "yahooQuotes"
let cols = "`sym`xdate`open`high`low`close`volume`adjclose"

kdb.ExecuteNonQuery(table+":([] sym:`$();xdate:`date$();open:`float$();high:`float$();low:`float$();close:`float$();volume:`int$();adjclose:`float$())" );

let urlleft,urlright=  "http://ichart.finance.yahoo.com/table.csv?s=","&a=00&b=3&c=1984&d=04&e=25&f=2011&ignore=.csv"

// now, download all the quotes
let datas = rawSym 
            |> List.fold(fun result (_,l) -> result |> List.append l) List.empty<string>
            |> List.iter ( fun s -> 
    try
        let d = webClient.DownloadString(urlleft + s + urlright).Split[|'\n'|] |> Array.toList |> List.tail
        d |> List.iter( fun l -> match l with
                                 | "" -> ()
                                 | _ -> let line = s+","+l
                                        kdb.ExecuteNonQuery(table+",:"+cols+"!(\"SDFFFFIF \";\",\")0:"+"\""+line+"\"")
            )
        
    with ex -> printfn "%A" ex
    )
kdb.ExecuteNonQuery("save `yahooQuotes")
;;
   
 // Isn't this cool?