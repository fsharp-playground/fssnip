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

fetchNugetInfo "EntityFramework" "6.0.0.0" 
|> function 
   | Some(title, true) -> Console.WriteLine(title + " is up-to-date")
   | Some(title, false) -> Console.WriteLine(title + " is outdated")
   | _ -> ignore()

