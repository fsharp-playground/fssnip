#r "Samples.DataStore.Freebase.dll"
open Samples.DataStore.Freebase

let Helsinki100 = 
   FreebaseData.GetDataContext().Commons.Business.``Stock exchanges``.Individuals.OMXH.``Companies traded``
   |> Seq.take 100
   |> Seq.map (fun company -> company.``Ticker symbol``)

Helsinki100 |> Seq.iter System.Console.WriteLine