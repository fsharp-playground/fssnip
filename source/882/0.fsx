// combine two options by applying a function to their values
let opSome f (lhs:'a option) (rhs:'a option) : 'a option =
    match lhs, rhs with
        | Some(l), Some(r)
            -> Some(f l r)
        | Some(l), None
            -> Some(l)
        | None, Some(r)
            -> Some(r)
        | _
            -> None

// curry the operator to produce a functions that "add" options of particular types
let addSomeStrings = opSome<string> (+)
let addSomeInts = opSome<int> (+)

// test with two options containing strings
let foo = Some("foo")
let bar = Some("bar")
addSomeStrings foo bar
addSomeStrings foo None
addSomeStrings None bar
addSomeStrings None None

let two = Some(2)
let three = Some(3)
addSomeInts two three
addSomeInts two None
addSomeInts None three
addSomeInts None None

// also works without the curried function
opSome (+) foo bar
opSome (+) foo None
opSome (+) None bar
opSome (+) None None
opSome (+) two three
opSome (+) two None
opSome (+) None three
opSome (+) None None
