open System

// Define a regular function
let double x = 2 * x

// Curry a function.  Basically saying "doubleFun is multiplication where the first argument is a 2"
// Also shows that operators are realy infix functions.  You can surround
// them with parens and they become normal functions
let doubleFun = (*) 2

double 2 // returns 4

doubleFun 4 // returns 8

// A type for a repID. Throws and error if it doesn't start with "ID")
// (ultra simple validation)
// Override ToString so it prints properly
type RepID(id: string) =
    do if not(id.StartsWith("ID")) then invalidArg "id" "RepID's must start with 'ID'"
    override this.ToString() = id

//creating a rep ID
let ben = RepID("ID1234")
Console.WriteLine(ben) // prints "ID1234"