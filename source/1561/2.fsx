#if INTERACTIVE
#r "FSharp.Data.TypeProviders"
#r "System.Data.Services.Client"
#else
module PackageManager
#endif

type Nuget = Microsoft.FSharp.Data.TypeProviders.ODataService<"https://nuget.org/api/v2">
let NugetConnection = Nuget.GetDataContext()
// NugetConnection.Credentials <- ...

let fetchNugetInfo id version = 
    try
        query { for package in NugetConnection.Packages do
                where (package.Id = id && package.Version = version)
                select (package.Title, package.IsLatestVersion)
                headOrDefault
        } |> Some
    with //Not found:
    | :? System.Data.Services.Client.DataServiceClientException -> None
    | :? System.Data.Services.Client.DataServiceQueryException -> None

let searchNuget name version = 
    (name, fetchNugetInfo name version)
    |> function 
       | n, Some(title, true) -> printfn "%s is up-to-date: %s" title n
       | n, Some(title, false) -> printfn "%s is outdated: %s" title n
       | n, _ ->  printfn "Not found: %s" n

//searchNuget "EntityFramework" "6.0.0.0" 
