open System.Text.RegularExpressions

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None   

let scores = [9;10;1;34;45;26;78;100;93]

type BucketRule = { Label : string; Rule : int -> bool}

type Filter = 
    | GreaterThan of int 
    | Between of int * int 
    | LessThan of int

let createFilter filterString =
    match filterString with    
    | Regex "^>(\d+)$" [min] -> GreaterThan(int min)
    | Regex "^(\d+)-(\d+)$" [min;max] -> Between(int min,int max)
    | Regex "^<(\d+)$" [max] -> LessThan(int max)
    | _ -> failwith "Invalid Filter"
    
let createPredicate = function
    | GreaterThan min -> fun n -> n > min
    | Between (min, max) -> fun n -> n >= min && n <= max
    | LessThan max -> fun n -> n < max
    
let createRule = createFilter >> createPredicate   
    
let createBucketRule filterString =
    { Label = filterString; Rule = createRule filterString }

let bucketRules = ["<21";"21-40";"41-60";">61"] 

let createBucket numbers bucketRule =
    let bucketContent = numbers |> List.filter bucketRule.Rule
    (bucketRule.Label, bucketContent)
        
let createBuckets numbers filterStrings  =
    filterStrings 
    |> List.map createBucketRule
    |> List.map (createBucket numbers)
    
let buckets = createBuckets scores bucketRules 