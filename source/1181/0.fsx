open System

let helloWorld () =
  match Some "Hello World!" with
  | Some value -> 
    match box value with
    | :? string as greeting -> printfn "%s" greeting
    | _ -> ()  
  | _ -> ()

helloWorld ()