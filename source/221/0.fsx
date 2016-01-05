module Test =
    let inline name (x : ^T) = (^T : (member Name : string) (x))

    type Person =
        {
            Name : string
            Age : int
        }

    let p : Person = { Name = "Tim"; Age = 29 }
    // error FS0001: The type 'Person' does not support any operators named 'get_Name'
    // printfn "%s" (name p)

    type Person2(name : string, age : int) =
        member this.Name = name
        member this.Age = age

    let p2 = Person2("Tim", 29)
    // works
    printfn "%s" (name p2)