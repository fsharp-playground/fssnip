open System.Collections

let inline pairs (collection : ^c when ^c :> IEnumerable and ^c : (member get_Item : 'a -> 'b)) : seq<'a * 'b> =
    seq {
        match collection.GetEnumerator() with
        | :? IDictionaryEnumerator as en ->
            while en.MoveNext() do
                yield unbox en.Key, unbox en.Value

        | en ->
            while en.MoveNext() do
                let key = unbox en.Current
                let value = (^c : (member get_Item : 'a -> 'b) (collection, key))
                yield key, value
    }


#r "System.Web"
open System.Collections.Specialized
open System.Web

for key, value in pairs (HttpUtility.ParseQueryString("hello=world&hello=tim&fsharp=cool")) do
    printfn "%s = %s" key value

let ht = Hashtable()
ht.["hello"] <- "world"
ht.["fsharp"] <- "cool"
for key, value in pairs ht do
    printfn "%s = %s" (unbox key) (unbox value)

let nvc = NameValueCollection()
nvc.["hello"] <- "world"
nvc.["fsharp"] <- "cool"
for key, value in pairs nvc do
    printfn "%s = %s" key value
