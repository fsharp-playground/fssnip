type myCls() = 
    //This this a new feature in VS2012,define a property is much easier than before.
    member val Value = 0 with get,set

    member this.Print() =
        printfn "%d" this.Value

    //Must initilaize the second construct by the default one before set the property Value
    new(value : int) as this = 
        new myCls()
        then
            this.Value <- value