    // A record type to hold name and age
    type Person = 
        { Name: string; 
          Age: int }

    // Partial active pattern with two argus
    let (|Young|_|) age (lst: list<Person>) =
        lst |> List.mapi(fun i _ -> (lst.Item i).Age) 
            |> List.tryFind(fun i -> i <= age)

    // A matching funtion
    let findYoungAge = function
        | Young 25 yr -> printfn "Found a young age of %d" yr
        | _ -> printfn "Couldn't find any ages younger than 25"

    // Construct a list of record values
    let listOfPerson = 
        [ {Name = "John";    Age = 29}; 
          {Name = "Simon";   Age = 24}; 
          {Name = "Shirley"; Age = 50} ]

    // "Found a young age of 24"
    findYoungAge listOfPerson
