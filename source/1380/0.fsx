#if INTERACTIVE
#r "FSharp.Data.dll"
#else
module GetStockInfo
#endif

 open FSharp.Data
 let data = FreebaseData.GetDataContext()
 let Helsinki100 = 
     data.Commons.Business.``Stock exchanges``.Individuals.OMXH.``Companies traded``
     |> Seq.take 100
     |> Seq.map (fun company -> company.``Ticker symbol``)

 let iterated = Helsinki100 |> Seq.iter System.Console.WriteLine