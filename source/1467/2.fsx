#if INTERACTIVE
#r "FSharp.Data.dll"
#r "FSharp.Data.DesignTime.dll"
#endif

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

// Freebase type provider creates types with special names that 
// represent individuals, collections etc. We can skip them.
let specialTypeName (n:string) =
  n.EndsWith("Individuals") || 
  n.EndsWith("Individuals10") || 
  n.EndsWith("Individuals100") || 
  n.EndsWith("IndividualsAZ") || 
  n.EndsWith("Data") || 
  n.EndsWith("DataCollection")

// For "level=2", this returns reasonable type names
// (representing the different entities in Freebase)

// For "level=3", this returns more fine-grained type
// names (including some individuals with special properties)
for n in getFreebaseTypes 2 do 
  if not (specialTypeName n) then 
    printfn "%s" n
