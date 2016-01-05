#if INTERACTIVE
#r @"Newtonsoft.Json.dll"
#endif

open Newtonsoft.Json.Linq

type Json =
    | JObj of Json seq
    | JProp of string * Json
    | JArr of Json seq
    | JVal of obj

let (!!) (o: obj) = JVal o

let rec toJson = function
    | JVal v -> new JValue(v) :> JToken
    | JProp(name, v) -> 
        match v with
        | JVal _ | JArr _ | JObj _ -> new JProperty(name, toJson v) :> JToken
        | JProp _ -> new JProperty(name, new JObject(toJson v)) :> JToken
    | JArr items -> new JArray(items |> Seq.map toJson) :> JToken
    | JObj props -> new JObject(props |> Seq.map toJson) :> JToken

// Suppose we want to create the following Json object:
// {
//     "id": 123,
//     "props": {
//         "prop": [
//             { "id": 1, v: 123 },
//             { "id": 2, v: 456 }
//         ]
//     }
// }
//
// Then the simplified lightweight Json creation will look like this:
let j =
    JObj [
        JProp("id", !! "123");
        JProp(
            "props", 
            JProp(
                "prop", 
                JArr [
                    JObj [JProp("id", !! 1); JProp("v", !! 123)];
                    JObj [JProp("id", !! 2); JProp("v", !! 456)]
                ]))
    ]

let json = toJson j

// Compare it to pure Json.Net.
let jj = 
    new JObject([
        new JProperty("id", "123");
        new JProperty(
            "props",
            new JObject(
                new JProperty(
                    "prop",
                    new JArray([
                        new JObject([new JProperty("id", 1); new JProperty("v", 123)]);
                        new JObject([new JProperty("id", 2); new JProperty("v", 456)])
                    ]))))
    ])
