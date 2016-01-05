open System

// Curry the printf function to always use a string
// (touch more like python/ruby print)
let print = printf "%s"

let traitorName = "Jackie"

let whoIsTheTraitor =
    match traitorName with
    | "Jason" -> "Jason is the traitor."
    | "Jackie" -> "Jackie is the traitor."
    | "Ben" -> "Ben is the traitor."
    | "Matt" -> "Matt is the traitor."
    | _ -> "There are no traitors."

print whoIsTheTraitor // prints "Jackie is the traitor."