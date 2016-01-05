open Newtonsoft.Json.Linq

let rec rename (f: string -> string) (json: JToken) =
    match json with
    | :? JProperty as prop -> 
        let name = f prop.Name
        let cont = rename f prop.Value
        new JProperty(name, cont :> obj) :> JToken
    | :? JArray as arr ->
        let cont = arr |> Seq.map (rename f)
        new JArray(cont) :> JToken
    | :? JObject as o ->
        let cont = o.Properties() |> Seq.map (rename f)
        new JObject(cont) :> JToken
    | _ -> json