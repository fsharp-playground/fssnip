//////////////////////////////////////////////////////////////////
// A compositional type system using generics and monads in F#. //
//////////////////////////////////////////////////////////////////
// A very limited, _toy_ project exploring traits, mixins       //
// and aspect oriented programming                              //
// by Zach Bray (http://www.zbray.com).                         //
//////////////////////////////////////////////////////////////////

(*[omit:(Class monad omitted.)]*)
module TypeSystem =

   type Class0() =
      do ()

   type Class1<'m1>(m1) =
      member c.Member1:'m1 = m1
      static member ( / ) (c:Class1<_>, m:'m) =
         c.Member1 m

   type Class2<'m1,'m2>(m1, m2) =
      member c.Member1:'m1 = m1
      member c.Member2:'m2 = m2
      static member ( / ) (c:Class2<_,_>, m:'m) =
         c.Member1 m
      static member ( / ) (c:Class2<_,_>, m:'m) =
         c.Member2 m

   type Class3<'m1,'m2,'m3>(m1, m2, m3) =
      member c.Member1:'m1 = m1
      member c.Member2:'m2 = m2
      member c.Member3:'m3 = m3
      static member ( / ) (c:Class3<_,_,_>, m:'m) =
         c.Member1 m
      static member ( / ) (c:Class3<_,_,_>, m:'m) =
         c.Member2 m
      static member ( / ) (c:Class3<_,_,_>, m:'m) =
         c.Member3 m

   type Class4<'m1,'m2,'m3,'m4>(m1, m2, m3, m4) =
      member c.Member1:'m1 = m1
      member c.Member2:'m2 = m2
      member c.Member3:'m3 = m3
      member c.Member4:'m4 = m4
      static member ( / ) (c:Class4<_,_,_,_>, m:'m) =
         c.Member1 m
      static member ( / ) (c:Class4<_,_,_,_>, m:'m) =
         c.Member2 m
      static member ( / ) (c:Class4<_,_,_,_>, m:'m) =
         c.Member3 m
      static member ( / ) (c:Class4<_,_,_,_>, m:'m) =
         c.Member4 m

   type Class5<'m1,'m2,'m3,'m4,'m5>(m1, m2, m3, m4, m5) =
      member c.Member1:'m1 = m1
      member c.Member2:'m2 = m2
      member c.Member3:'m3 = m3
      member c.Member4:'m4 = m4
      member c.Member5:'m5 = m5
      static member ( / ) (c:Class5<_,_,_,_,_>, m:'m) =
         c.Member1 m
      static member ( / ) (c:Class5<_,_,_,_,_>, m:'m) =
         c.Member2 m
      static member ( / ) (c:Class5<_,_,_,_,_>, m:'m) =
         c.Member3 m
      static member ( / ) (c:Class5<_,_,_,_,_>, m:'m) =
         c.Member4 m
      static member ( / ) (c:Class5<_,_,_,_,_>, m:'m) =
         c.Member5 m

   type Class5 with
      static member ( - ) (x:Class5< ('m -> 'a) , _, _, _, _>, m:'m) = 
         Class4(x.Member2, x.Member3, x.Member4, x.Member5)
      static member ( - ) (x:Class5<_, ('m -> 'a), _, _, _>, m:'m) = 
         Class4(x.Member1, x.Member3, x.Member4, x.Member5)
      static member ( - ) (x:Class5<_, _, ('m -> 'a), _, _>, m:'m) = 
         Class4(x.Member1, x.Member2, x.Member4, x.Member5)
      static member ( - ) (x:Class5<_, _, _, ('m -> 'a), _>, m:'m) = 
         Class4(x.Member1, x.Member2, x.Member3, x.Member5)
      static member ( - ) (x:Class5<_, _, _, _, ('m -> 'a)>, m:'m) = 
         Class4(x.Member1, x.Member2, x.Member3, x.Member4)

   type Class4 with
      static member ( + ) (x:Class4<_,_,_,_>, y:Class1<_>) = 
         Class5(x.Member1, x.Member2, x.Member3, x.Member4, y.Member1)

      static member ( - ) (x:Class4< ('m -> 'a) , _, _, _>, m:'m) = 
         Class3(x.Member2, x.Member3, x.Member4)
      static member ( - ) (x:Class4<_, ('m -> 'a), _, _>, m:'m) = 
         Class3(x.Member1, x.Member3, x.Member4)
      static member ( - ) (x:Class4<_, _, ('m -> 'a), _>, m:'m) = 
         Class3(x.Member1, x.Member2, x.Member4)
      static member ( - ) (x:Class4<_, _, _, ('m -> 'a)>, m:'m) = 
         Class3(x.Member1, x.Member2, x.Member3)

   type Class3 with
      static member ( + ) (x:Class3<_,_,_>, y:Class1<_>) = 
         Class4(x.Member1, x.Member2, x.Member3, y.Member1)
      static member ( + ) (x:Class3<_,_,_>, y:Class2<_,_>) = 
         Class5(x.Member1, x.Member2, x.Member3, y.Member1, y.Member2)

      static member ( - ) (x:Class3< ('m -> 'a) , _, _>, m:'m) = 
         Class2(x.Member2, x.Member3)
      static member ( - ) (x:Class3<_, ('m -> 'a), _>, m:'m) = 
         Class2(x.Member1, x.Member3)
      static member ( - ) (x:Class3<_, _, ('m -> 'a)>, m:'m) = 
         Class2(x.Member1, x.Member2)

   type Class2 with   
      static member ( + ) (x:Class2<_,_>, y:Class1<_>) = 
         Class3(x.Member1, x.Member2, y.Member1)
      static member ( + ) (x:Class2<_,_>, y:Class2<_,_>) = 
         Class4(x.Member1, x.Member2, y.Member1, y.Member2)
      static member ( + ) (y:Class2<_,_>, x:Class3<_,_,_>) = 
         Class5(x.Member1, x.Member2, x.Member3, y.Member1, y.Member2)

      static member ( - ) (x:Class2< ('m -> 'a) , _>, m:'m) = 
         Class1(x.Member2)
      static member ( - ) (x:Class2<_, ('m -> 'a) >, m:'m) = 
         Class1(x.Member1)

   type Class1 with
      static member ( + ) (x:Class1<_>, y:Class1<_>) = 
         Class2(x.Member1, y.Member1)
      static member ( + ) (y:Class1<_>, x:Class2<_,_>) = 
         Class3(x.Member1, x.Member2, y.Member1)
      static member ( + ) (y:Class1<_>, x:Class3<_,_,_>) = 
         Class4(x.Member1, x.Member2, x.Member3, y.Member1)
      static member ( + ) (y:Class1<_>, x:Class4<_,_,_,_>) = 
         Class5(x.Member1, x.Member2, x.Member3, x.Member4, y.Member1)

      static member ( - ) (x:Class1< ('m -> 'a) >, m:'m) = Class0()

   type Class0 with
      static member ( + ) (x:Class0, y:Class1<_>) = y
      static member ( + ) (x:Class0, y:Class2<_,_>) = y
      static member ( + ) (x:Class0, y:Class3<_,_,_>) = y
      static member ( + ) (x:Class0, y:Class4<_,_,_,_>) = y
      static member ( + ) (x:Class0, y:Class5<_,_,_,_,_>) = y

   type ClassBuilder() =
      member inline b.Yield f = Class1(f)
      member inline b.YieldFrom x = x
      member inline b.Combine(x, y) = x + y
      member inline b.Delay f = f()
   
   let Class = ClassBuilder()

(*[/omit]*)


open System
open TypeSystem

//////////////////////////////////////////////////////////////////
// BASICS                                                       //
//////////////////////////////////////////////////////////////////

// Member symbols are defined as single cases.
type FirstName = |FirstName
type LastName = |LastName
type Name = |Name

// Class constructors are created using the Class monad.
// In this example only 5 members are supported.
let person firstName lastName = Class {
   // Each member function is yielded
   yield function FirstName -> firstName
   yield function LastName -> lastName
   yield function Name -> firstName + " " + lastName
}

// Members symbols can have arguments (and these arguments can be generic).
type 'a AddManager = |AddManager of 'a
type Managers = |Managers

// Class constructors can be generic
let worker<'a>() = Class {
   // Mutable state can be kept inside reference cells.
   let managers = ref List.empty<'a>
   yield function Managers -> !managers
   // A member that takes a parameter
   yield function AddManager newManager -> 
                  managers := newManager :: !managers
}

// Class constructors can be composed
let employee<'a> firstName secondName = Class {
   // We can yield the members of one class...
   yield! person firstName secondName
   // ... then another
   yield! worker<'a>()
}

// We construct instances by passing the parameters into
// the constuctor
let testConstruction =
   let rupert = employee<unit> "Rupert" "Maddog"
   let becca = employee "Becca" "Brooked"
   let dave = employee "Dave" "Kameroon"

   // We access members by using the (/) operator
   becca / AddManager(rupert)
   dave / AddManager(becca)

   for manager in dave / Managers do
      printfn "%s" (manager / Name)


//////////////////////////////////////////////////////////////////
// MIXINS & ADAPTORS                                            //
//////////////////////////////////////////////////////////////////

// Class constructors can also be composed using 
// the (+) operator which allows us to use mixins a la Scala
// http://www.scala-lang.org/node/117

// Iterator members
type HasNext = |HasNext
type Next = |Next

// Rich iterator members
type 'a ForEach = |ForEach of ('a -> unit)

// A rich iterator provides a foreach wrapper
// around the HasNext and Next members
let inline richIterator x = 
   Class {
      yield function ForEach f ->
                     while x / HasNext do
                        f (x / Next)
   }

// Here we construct a basic string iterator
let stringIterator (str:string) = Class {
   let i = ref 0
   yield function HasNext -> !i < str.Length
   yield function Next ->
                  let c = str.[!i]
                  incr i
                  c
}

// Here we construct a mixin of the string iterator
// and the rich iterator
let richStringIterator str =
   let iter = stringIterator str
   // We combine the rich iterator interface with
   // the existing interface here using the (+) operator
   richIterator iter + iter

// We can also choose to use the rich iterator constructor
// as an adaptor rather than a mixin by omitting the
// composition with the original iter
let onlyRichStringIterator str =
   richIterator (stringIterator str)

// We can use either the ForEach member or the HasNext and
// Next members of a richStringIterator to print a string
let testRSI =
   // Using rich interface
   let iter = richStringIterator "Ordered generic parameters suck!"
   iter / ForEach (printf "%c")
   printfn ""

   // Using basic interface
   let iter2 = richStringIterator "Arbitrary metrics suck!"
   while iter2 / HasNext do
      printf "%c" (iter2 / Next)
   printfn ""

// If we use the adapter method we can only use the rich interface
let testORSI =
   // Using rich interface still works!
   let iter = onlyRichStringIterator "Type safety rules!"
   iter / ForEach (printf "%c")
   printfn ""

   // Using basic interface will _not_ compile!
   (*
   let iter2 = onlyRichStringIterator "Type safety rules!"
   while iter2 / HasNext do
      printf "%c" (iter2 / Next)
   printfn ""
   *)

//////////////////////////////////////////////////////////////////
// ASPECT-ORIENTED                                              //
//////////////////////////////////////////////////////////////////

// In addition to adding new members using the (+) operator we
// can also hide members using the (-) operator.

// This means we can do some simple aspect oriented programming
// http://en.wikipedia.org/wiki/Aspect-oriented_programming


// Here we create the interface to a bank account...
type Balance = |Balance
type Deposit = |Deposit of decimal
type Withdraw = |Withdraw of decimal

// ... and its constructor
let account name = Class {
   let balance = ref 0m
   yield function Name -> name
   yield function Balance -> !balance
   yield function Deposit x -> balance := !balance + x
   yield function Withdraw x -> balance := !balance - x
}

// Here we create a helper that will run some code before a given
// member is accessed.
let inline beforeAccess f (property:'a) x =
   Class {
      yield fun (_:'a) ->
         f()
         x / property
   } + (x - property)

// Here we create a function that logs before a property is accessed
let inline logAccess property x =
   x |> beforeAccess (fun () -> printfn "%A Accessed!" property) property

// Here we create a new constructor for an account that logs balance
// requests.
let loggingAccount name =
   account name |> logAccess Balance

let testLoggingAccount =
   let acc = loggingAccount "Zach's current account"
   let illicitFunds = 1000000m
   acc / Deposit illicitFunds
   printfn "Zach's account balance is: %f" (acc / Balance)
   // prints:
   // > Balance Accessed!
   // > Zach's account balance is: 1000000.000000

// We can re-use the same block of code to log when a persons name is accessed.

// Here we create a new constructor for a person that logs when their name
// is accessed
let loggingPerson fName sName =
   person fName sName |> logAccess Name

let testLoggingPerson =
   let zach = loggingPerson "Zach" "Bray"
   printf "My name is: %s" (zach / Name)
   // prints:
   // > Name Accessed!
   // > My name is: Zach Bray

Console.ReadLine() |> ignore<string>