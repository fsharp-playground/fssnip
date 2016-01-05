open System
open System.Collections.Generic

// Dictionary Demo Script for a set of Nested Data Records
// In this example the dictionary value is defined to contain a 
// variable number of records of the specified type.
// (In this example the record is just an integer value)  
 
// Define Record Types 
type  tinfo = { 
    num:int;
}

let populate_dict = 
    // Populate a dictionary with a nested set of records and display contents
    // Define a dictionary to hold nested record
    let mydict = new Dictionary<string, tinfo list>()
    
    // Add some arbitrary data 
    mydict.Add("bb", [{num=2;}; {num=3;}])
    mydict.Add("a", [{num=1;}])
    mydict.Add("ccc", [{num=4;}; {num=5;}; {num = 6;}])
    mydict


let print_dict (mydict)= 
    // Print Contents ( Keys and Values) in dictionary
    mydict 
    // Sort by key
    |> Seq.sortBy(fun ( KeyValue(k,v)) -> k)
    // Iterate over dict entries
    |> Seq.iter (fun  (KeyValue(k,v)) ->  
                    printfn "Dict Key: %s" k; 
                    v |>
                    // Now iterate over record list
                    Seq.iter (fun v -> 
                        printfn "Dict Values: %A;" v) )
    

// Main Entry Point
let main() =

    printfn "\n>>>>>> F# Dictionary Demo  <<<<< \n" 

    let d = populate_dict
    print_dict(d)

    printfn "\n>>>>>> Done  <<<<< \n" 

    printfn "\n>>>>>> Press Any Key to Exit! \n"
    System.Console.ReadLine() |> ignore

main()