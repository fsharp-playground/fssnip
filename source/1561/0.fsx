#if INTERACTIVE
#r "FSharp.Data.TypeProviders"
#r "System.Data.Services.Client"
#else
module PackageManager
#endif

type Nuget = Microsoft.FSharp.Data.TypeProviders.ODataService<"https://nuget.org/api/v2">
let NugetConnection = Nuget.GetDataContext()
let fetchNugetInfo id version = 
    query { for package in NugetConnection.Packages do
            where (package.Id = id && package.Version = version)
            select package.Title
    }
//fetchNugetInfo "EntityFramework" "6.0.0.0" |> Seq.iter (Console.WriteLine)
