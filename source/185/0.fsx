// [snippet:Extending the Fold function in terms of itself]
module List =

    //Executes a fold operation within a list returning a two-dimension tuple with
    //the first element being the result of the fold and the second being the count of the
    //processed elements.
    let public foldc fold first source  =
       source 
       |> List.fold(fun (prev,count) c -> (fold prev c,count + 1)) (first,0)

    //Executes a fold operation within a list passing as parameter of the folder function 
    //the zero based index of each element.
    let public foldi fold first source  =
       source 
       |> List.fold(fun (prev,i) c -> (fold i prev c,i + 1)) (first,0)
       |> fst

    //Executes a fold operation within a list passing as parameter of the folder function 
    //the zero based index of each element and returning a two-dimension tuple with
    //the first element being the result of the fold and the second being the count of the
    //processed elements.
    let public foldic fold first source  =
        source 
        |> List.fold(fun (prev,i) c -> (fold i prev c, i + 1)) (first,0)
// [/snippet] 


// [snippet:Examples]
let foldc_result = [1 .. 9] 
                   |> List.foldc (+) 0
                   //(45,9)

let foldi_result = [1 .. 9] 
                   |> List.foldi (fun i (s1,s2) c -> (s1 + (2.0 ** float c), 
                                                      s2 + (2.0 ** float i))) (0.0,0.0) 
                   //(1022.0, 511.0)

let foldic_result = [1 .. 9] 
                   |> List.foldic (fun i (s1,s2) c -> (s1 + (2.0 ** float c), 
                                                       s2 + (2.0 ** float i))) (0.0,0.0) 
                    //(1022.0, 511.0,9)        
// [/snippet]           