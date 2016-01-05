// NOTE: This code is using the type provider internals and so it needs
// reference to both "FSharp.Data.dll" and "FSharp.Data.DesignTime.dll"
open System
open FSharp.Data
open ProviderImplementation

// Initialize the type provider & get ITypeProvider implementation
let loc = typeof<FSharp.Data.CsvFile>.Assembly.Location
let fl _ = false
let config = CompilerServices.TypeProviderConfig(fl, RuntimeAssembly=loc)
let fb = new FreebaseTypeProvider(config)
let tp = (fb :> CompilerServices.ITypeProvider)

// Given a System.Type, get the names of all its nested 
// types (recursively until 'level' reaches zero)
let rec getTypeNames level (typ:System.Type) = seq {
  for nested in typ.GetNestedTypes() do
    yield nested.Name 
    if level > 0 then 
      yield! getTypeNames (level - 1) nested }

// Get the types in the Freebase namespace 
// (recursively using 'getTypeNames')
let getFreebaseTypes level = 
  set [ for ns in tp.GetNamespaces() do
          for t in ns.GetTypes() do
            yield! t |> getTypeNames level ]

// For "level=2", this returns reasonable type names
// (representing the different entities in Freebase)
for n in getFreebaseTypes 2 do 
  printfn "%s" n
