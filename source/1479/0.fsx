
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.WindowsAzure.ServiceRuntime

type Person(partitionKey, rowKey, name) =
    inherit TableEntity(partitionKey, rowKey)
    new(name) = Person("defaultPartition", System.Guid.NewGuid().ToString(), name)
    new() = Person("")
    member val Name = name with get, set

(*
    Add this into ServiceDefinition.csdef <WorkerRole>:
    <ConfigurationSettings>
      <Setting name="TableStorageConnectionString" />
    </ConfigurationSettings>

    Add this into ServiceConfiguration.*.csdef <ConfigurationSettings>:
    <Setting name="TableStorageConnectionString" value="UseDevelopmentStorage=true" />
*)

let doAction tableName operation = 
    let account = 
        "TableStorageConnectionString"
        |> RoleEnvironment.GetConfigurationSettingValue 
        |> CloudStorageAccount.Parse

    //memoize this
    let client = account.CreateCloudTableClient()
    let table = client.GetTableReference(tableName)

    async {
        let! created = table.CreateIfNotExistsAsync() |> Async.AwaitTask
        return! table.ExecuteAsync(operation) |> Async.AwaitTask
    }

let CreatePerson() = 
    Person("Tuomas")
    |> TableOperation.Insert //Insert, Delete, Replace, etc.
    |> doAction "MyStorageTable"
    |> Async.RunSynchronously
