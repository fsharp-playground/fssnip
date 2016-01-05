#r "System.Data.Services.Client"
#r "FSharp.Data.TypeProviders"

open Microsoft.FSharp.Data.TypeProviders

type stackOverflowData = 
    ODataService<"http://data.stackexchange.com/stackoverflow/atom">
let context = stackOverflowData.GetDataContext()