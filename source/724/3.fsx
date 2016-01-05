//---------------------------------------------------------------
// Overview
//
// Below is a set of exercises designed to get you familiar 
// with F#. By the time you're done, you'll have a basic 
// understanding of the syntax of F# and learn a little more
// about functional programming in general.
//
// Answering Problems
// 
// This is where the fun begins! Each dashed section contains an 
// example designed to teach you a lesson about the F# language. 
// If you highlight the code in an example and execute it (use 
// Ctrl+Enter or the run button) it will initially fail. Your
// job is to fill in the blanks to make it pass. With each
// passing section, you'll learn more about F#, and add another
// weapon to your F# programming arsenal.
//
// Start by highlighitng the section below and running it. Once
// you see it fail, replace the __ with 2 to make it pass.
//---------------------------------------------------------------

// ---- about asserts -------------------------------------------

let expected_value = 1 + 1
let actual_value = __

AssertEquality expected_value actual_value

//Easy, right? Try the next one.

//---------------------------------------------------------------
 
// ---- more about asserts --------------------------------------

AssertEquality "foo" __

//---------------------------------------------------------------

//---------------------------------------------------------------
// About Let
//
// The let keyword is one of the most fundamental parts of F#.
// You'll use it in almost every line of F# code you write, so
// let's get to know it well! (no pun intended)
//---------------------------------------------------------------

// ---- let binds a name to a value -----------------------------

let x = 50
        
AssertEquality x __
    
//---------------------------------------------------------------

// ---- let infers the type of values when it can ---------------

(* In F#, values created with let are inferred to have a type like
   "int" for integer values, "string" for text values, and "bool" 
   for true or false values. *)

let x = 50
let typeOfX = x.GetType()
AssertEquality typeOfX typeof<int>

let y = "a string"
let expectedType = y.GetType()
AssertEquality expectedType typeof<FILL_ME_IN>

//---------------------------------------------------------------

// ---- you can make the types explicit -------------------------

let (x:int) = 42
let typeOfX = x.GetType()

let y:string = "forty two"
let typeOfY = y.GetType()

AssertEquality typeOfX typeof<FILL_ME_IN>
AssertEquality typeOfY typeof<FILL_ME_IN>

(* You don't usually need to provide explicit type annotations 
   types for local varaibles, but type annotations can come in 
   handy in other contexts as you'll see later. *)

//---------------------------------------------------------------

// ---- floats and ints -----------------------------------------

(* Depending on your background, you may be surprised to learn that
    in F#, integers and floating point numbers are different types. 
    In other words, the following is true. *)
let x = 20
let typeOfX = x.GetType()

let y = 20.0
let typeOfY = y.GetType()

//you don't need to modify these
AssertEquality typeOfX typeof<int>
AssertEquality typeOfY typeof<float>

//If you're coming from another .NET language, float is F# slang for
//the double type.
   
//---------------------------------------------------------------

// ---- modifying the value of variables ------------------------

let mutable x = 100
x <- 200

AssertEquality x __

//---------------------------------------------------------------

// ---- you can't modify a value if it isn't mutable ------------

let x = 50

//What happens if you try to uncomment and run the following line of code?
//(look at the output in the output window)
//x <- 100

//NOTE: Although you can't modify immutable values, it is 
//      possible to reuse the name of a value in some cases 
//      using "shadowing".
let x = 100
 
AssertEquality x __

//---------------------------------------------------------------

//---------------------------------------------------------------
// About Functions
//
// Now that you've seen how to bind a name to a value with let,
// you'll learn to use the let keyword to create functions.
//---------------------------------------------------------------

// ---- creating functions with let -----------------------------

(* By default, F# is whitespace sensitive. For functions, this 
   means that the last line of a function is its return value,
   and the body of a function is denoted by indentation. *)

let add x y =
    x + y

let result1 = add 2 2
let result2 = add 5 2

AssertEquality result1 __
AssertEquality result2 __

//---------------------------------------------------------------

// ---- nesting functions ---------------------------------------

let quadruple x =    
    let double x =
        x * 2

    double(double(x))

let result = quadruple 4
AssertEquality result __

//---------------------------------------------------------------

// ---- adding type annotations ---------------------------------

(* Sometimes you need to help F#'s type inference system out with
   an explicit type annotation *)

let sayItLikeAnAuctioneer (text:string) =
    text.Replace(" ", "")

let auctioneered = sayItLikeAnAuctioneer "going once going twice sold to the lady in red"
AssertEquality auctioneered __

//TRY IT: What happens if you remove the type annotation on text?

//---------------------------------------------------------------

// ---- variables in the parent scope can be accessed -----------

let suffix = "!!!"

let caffinate (text:string) =
    let exclaimed = text + suffix
    let yelled = exclaimed.ToUpper()
    yelled.Trim()

let caffinatedReply = caffinate "hello there"

AssertEquality caffinatedReply __

(* NOTE: Accessing the suffix variable in the nested caffinate function 
         is known as a closure. 
         
         See http://en.wikipedia.org/wiki/Closure_(computer_science) 
         for more about about closure. *)

//---------------------------------------------------------------

//---------------------------------------------------------------
// About the Order of Evaluation
//
// Sometimes you'll need to be explicit about the order in which
// functions are evaluated. F# offers a couple mechanisms for
// doing this.
//---------------------------------------------------------------

// ---- using parenthesis to control the order of operation -----

let add x y =
    x + y

let result = add (add 5 8) (add 1 1)

AssertEquality result __

(* TRY IT: What happens if you remove the parensthesis?*)

//---------------------------------------------------------------

// ---- the backward pipe operator can also help with grouping --

let add x y =
    x + y

let double x =
    x * 2

let result = double <| add 5 8

AssertEquality result __

//---------------------------------------------------------------

//---------------------------------------------------------------
// About Unit
//
// The unit type is a special type that represents the lack of
// a value. It's similar to void in other languages, but unit
// is actually considered to be a type in F#.
//---------------------------------------------------------------

// ---- unit is used when there is no return value --------------
let sendData data =
    //...pretend we are sending the data to the server...
    ()

let x = sendData "data"
AssertEquality x __ //Don't overthink this

//---------------------------------------------------------------

// ---- parameterless fucntions take unit as their argument -----

let sayHello() =
    "hello"

let result = sayHello()
AssertEquality result __

//---------------------------------------------------------------

//---------------------------------------------------------------
// Tuples
//
// Tuples are used to easily group together values in F#. They're 
// another fundamental construct of the language.
//---------------------------------------------------------------

// ---- creating tuples -----

let items = ("apple", "dog")

AssertEquality items ("apple", __)

//---------------------------------------------------------------

// ---- accessing tuple elements --------------------------------

let items = ("apple", "dog")
 
let fruit = fst items
let animal = snd items
 
AssertEquality fruit __
AssertEquality animal __

//---------------------------------------------------------------
       
// ---- accessing tuple elements with pattern matching ----------

(* fst and snd are useful in some situations, but they only work with
   tuples containing two elements. It's usually better to use a 
   technique called pattern matching to access the values of a tuple. 
    
   Pattern matching works with tuples of any arity, and it allows you to 
   simultaneously break apart the tuple while assigning a name to each 
   value. Here's an example. *)

let items = ("apple", "dog", "Mustang")

let fruit, animal, car = items

AssertEquality fruit __
AssertEquality animal __
AssertEquality car __

//---------------------------------------------------------------
        
// ---- ignoring values when pattern matching -------------------
       
let items = ("apple", "dog", "Mustang")

let _, animal, _ = items

AssertEquality animal __
    
//---------------------------------------------------------------
        
// ---- using tuples to return multiple values from a function --

let squareAndCube x =
    (x ** 2.0, x ** 3.0)

let squared, cubed = squareAndCube 3.0


AssertEquality squared __
AssertEquality cubed __

(* THINK ABOUT IT: Is there really more than one return value?
                   What type does the squareAndCube function
                   return? *)

//---------------------------------------------------------------

// ---- the truth behind multiple return values ------------------

let squareAndCube x =
    (x ** 2.0, x ** 3.0)
            
let result = squareAndCube 3.0
       
AssertEquality result __
    
//---------------------------------------------------------------

//---------------------------------------------------------------
// Branching
//
// Branching is used to tell a program to conditionally perform
// an operation. It's another fundamental part of F#.
//---------------------------------------------------------------

// ---- basic if statements -------------------------------------

let isEven x =
    if x % 2 = 0 then
        "it's even!"
    else
        "it's odd!"
        
let result = isEven 2                
AssertEquality result __

//---------------------------------------------------------------

// ---- if statements return values -----------------------------
    
(* In languages like C++, Java, and C# if statements do not yield 
  results; they can only cause side effects. If statements in F# 
  return values due to F#'s functional programming roots. *)
   
let result = 
    if 2 = 3 then
        "something is REALLY wrong"
    else
        "math is workng!"

AssertEquality result __

//---------------------------------------------------------------

// ---- branching with pattern matching -------------------------
 
let isApple x =
    match x with
    | "apple" -> true
    | _ -> false

let result1 = isApple "apple"
let result2 = isApple ""

AssertEquality result1 __
AssertEquality result2 __

//---------------------------------------------------------------

// ---- using tuples with if statements quickly becomes clumsy --
        
let getDinner x = 
    let name, foodChoice = x
    
    if foodChoice = "veggies" || foodChoice ="fish" || 
       foodChoice = "chicken" then
        sprintf "%s doesn't want red meat" name
    else
        sprintf "%s wants 'em some %s" name foodChoice
 
let person1 = ("Chris", "steak")
let person2 = ("Dave", "veggies")

AssertEquality (getDinner person1) __
AssertEquality (getDinner person2) __

//---------------------------------------------------------------

// ---- pattern matching with tuples is much nicer --------------
    
let getDinner x =
    match x with
    | (name, "veggies")
    | (name, "fish")
    | (name, "chicken") -> sprintf "%s doesn't want red meat" name
    | (name, foodChoice) -> sprintf "%s wants 'em some %s" name foodChoice 
    
let person1 = ("Bob", "fish")
let person2 = ("Sally", "Burger")

AssertEquality (getDinner person1) __
AssertEquality (getDinner person2) __

//---------------------------------------------------------------

//---------------------------------------------------------------
// About Lists
//
// Lists are important building blocks that you'll use frequently
// in F# programming. They are used to group arbitrarily large 
// sequences of values. It's very common to store values in a 
// list and perform operations across each value in the 
// list.
//---------------------------------------------------------------

// ---- creating lists ------------------------------------------

let list = ["apple"; "pear"; "grape"; "peach"]

//Note: The list data type in F# is a singly linked list, 
//      so indexing elements is O(n). 
 
AssertEquality list.Head __
AssertEquality list.Tail __
AssertEquality list.Length __

       
(* .NET developers coming from other languages may be surprised
   that F#'s list type is not the same as the base class library's
   List<T>. In other words, the following assertion is true *)

let dotNetList = new List<string>()

//you don't need to modify the following line
AssertInequality (list.GetType()) (dotNetList.GetType())

//---------------------------------------------------------------

// ---- building new lists---------------------------------------

let first = ["grape"; "peach"]
let second = "pear" :: first
let third = "apple" :: second

//Note: "::" is known as "cons"

AssertEquality ["apple"; "pear"; "grape"; "peach"] third
AssertEquality second __
AssertEquality first __

//What happens if you uncomment the following?

//first.Head <- "apple"
//first.Tail <- ["peach"; "pear"]

//THINK ABOUT IT: Can you change the contents of a list once it 
//                has been created?

//---------------------------------------------------------------

// ---- concatenating lists -------------------------------------

let first = ["apple"; "pear"; "grape"]
let second = first @ ["peach"]

AssertEquality first __
AssertEquality second __

(* THINK ABOUT IT: In general, what performs better for building lists, 
   :: or @? Why?
   
   Hint: There is no way to modify "first" in the above example. It's
   immutable. With that in mind, what does the @ function have to do in
   order to append ["peach"] to "first" to create "second"? *)

//---------------------------------------------------------------

// ---- creating lists with a range------------------------------

let list = [0..4]

AssertEquality list.Head __
AssertEquality list.Tail __

//---------------------------------------------------------------

// ---- creating lists with comprehensions-----------------------

let list = [for i in 0..4 do yield i ]
                            
AssertEquality list __

//---------------------------------------------------------------
    
// ---- comprehensions with conditions --------------------------

let list = [for i in 0..10 do 
                if i % 2 = 0 then yield i ]
                    
AssertEquality list __

//---------------------------------------------------------------

// ---- transforming lists with map -----------------------------

let square x =
    x * x

let original = [0..5]
let result = List.map square original

AssertEquality original __
AssertEquality result __

//---------------------------------------------------------------

// ---- filtering lists with where ------------------------------

let isEven x =
    x % 2 = 0

let original = [0..5]
let result = List.filter isEven original

AssertEquality original __
AssertEquality result __

//---------------------------------------------------------------

// ---- dividing lists with partition ---------------------------

let isOdd x =
    not(x % 2 = 0)

let original = [0..5]
let result1, result2 = List.partition isOdd original

AssertEquality result1 __
AssertEquality result2 __

//---------------------------------------------------------------

(* Note: There are many other useful methods in the List module. Check them
   via intellisense in Visual Studio by typing '.' after List, or online at
   http://msdn.microsoft.com/en-us/library/ee353738.aspx *)

//---------------------------------------------------------------
// Pipelining
//
// The forward pipe operator is one of the most commonly used
// symbols in F# programming. You can use it combine operations
// on lists and other data structures in a readable way.
//---------------------------------------------------------------


// ---- square even numbers with separate statementes -----------

let square x =
    x * x

let isEven x =
    x % 2 = 0

(* One way to combine operations is by using separate statements.
   However, this is can be clumsy since you have to name each result. *)

let numbers = [0..5]

let evens = List.filter isEven numbers
let result = List.map square evens

AssertEquality result __

//---------------------------------------------------------------

// ---- square even numbers with parens -------------------------

(* You can avoid this problem by using parens to pass the result of one
   funciton to another. This can be difficult to read since you have to 
   start from the innermost function and work your way out. *)

let numbers = [0..5]

let result = List.map square (List.filter isEven numbers)

AssertEquality result __

//---------------------------------------------------------------

// ---- square even numbers with  the pipeline operator ---------

(* In F#, you can use the pipeline operator to get the benefit of the 
   parens style with the readablity of the statement style. *)

let result =
    [0..5]
    |> List.filter isEven
    |> List.map square

AssertEquality result __

//---------------------------------------------------------------

// ---- how the pipe operator is defined ------------------------

let (|>) x y =
    y x

let result =
    [0..5]
    |> List.filter isEven
    |> List.map square

AssertEquality result __

//---------------------------------------------------------------

//---------------------------------------------------------------
// Arrays
//
// Like lists, arrays are another basic container type in F#.
//---------------------------------------------------------------

// ---- creating arrays -----------------------------------------

let fruits = [| "apple"; "pear"; "peach"|]

AssertEquality fruits.[0] __
AssertEquality fruits.[1] __
AssertEquality fruits.[2] __

//---------------------------------------------------------------

// ---- arrays are mutable --------------------------------------

let fruits = [| "apple"; "pear" |]
fruits.[1] <- "peach"

AssertEquality fruits __

//---------------------------------------------------------------

// ---- you can create arrays with comprehensions ---------------

let numbers = 
    [| for i in 0..10 do 
           if i % 2 = 0 then yield i |]

AssertEquality numbers __

//---------------------------------------------------------------

// ---- you can also perform operations on arrays ---------------

let cube x =
    x * x * x

let original = [| 0..5 |]
let result = Array.map cube original

AssertEquality original __
AssertEquality result __

(* See more Array methods at
   http://msdn.microsoft.com/en-us/library/ee370273.aspx *)

//---------------------------------------------------------------

//---------------------------------------------------------------
// .NET Collections
//
// Since F# is bulit for seamless interop with other CLR 
// languages, you can use all of the basic .NET collections types
// you're already familiar with if you're a C# or VB programmer.
//---------------------------------------------------------------

// ---- creating .NET lists -------------------------------------

let fruits = new List<string>()

fruits.Add("apple")
fruits.Add("pear")

AssertEquality fruits.[0] __
AssertEquality fruits.[1] __

//---------------------------------------------------------------

// ---- creating .NET dictionaries ------------------------------

let addressBook = new Dictionary<string, string>()

addressBook.["Chris"] <- "Ann Arbor"
addressBook.["SkillsMatter"] <- "London"

AssertEquality addressBook.["Chris"] __
AssertEquality addressBook.["SkillsMatter"] __

//---------------------------------------------------------------

// ---- you can use combinators with .NET types  ----------------

let addressBook = new Dictionary<string, string>()

addressBook.["Chris"] <- "Ann Arbor"
addressBook.["SkillsMatter"] <- "London"

let verboseBook = 
    addressBook
    |> Seq.map (fun kvp -> sprintf "Name: %s - City: %s" kvp.Key kvp.Value)
    |> Seq.toArray

//NOTE: The seq type in F# is an alias for .NET's IEnumerable interface
//      Like the List and Array module, the Seq module contains functions 
//      that you can combine to perform operations on types implementing 
//      seq/IEnumerable. The methods found in these modules are known as
//      combinators

AssertEquality verboseBook.[0] __
AssertEquality verboseBook.[1] __

//---------------------------------------------------------------

// ---- skipping elements ---------------------------------------

let original = [0..5]
let result = Seq.skip 2 original

AssertEquality result __

//---------------------------------------------------------------

// ---- finding the max -----------------------------------------

let values = new List<int>()

values.Add(11)
values.Add(20)
values.Add(4)
values.Add(2)
values.Add(3)

let result = Seq.max values

AssertEquality result __
    
//---------------------------------------------------------------

// ---- finding the max using a condition -----------------------

let getNameLength (name:string) =
    name.Length

let names = [| "Harry"; "Lloyd"; "Nicholas"; "Mary"; "Joe"; |]
let result = Seq.maxBy getNameLength names 

AssertEquality result __

//---------------------------------------------------------------

//---------------------------------------------------------------
// Looping
//
// While it's more common in F# to use the Seq, List, or Array
// modules to perform looping operations, you can still fall 
// back on traditional imperative looping techniques that you may 
// be more familiar with.
//---------------------------------------------------------------

// ---- looping over a list -------------------------------------

let values = [0..10]

let mutable sum = 0
for value in values do
    sum <- sum + value

AssertEquality sum __
       
//---------------------------------------------------------------

// ---- looping with expressions --------------------------------

let mutable sum = 0

for i = 1 to 5 do
    sum <- sum + i

AssertEquality sum __

//---------------------------------------------------------------

// ---- looping with while --------------------------------------

let mutable sum = 1

while sum < 10 do
    sum <- sum + sum

AssertEquality sum __

(* NOTE: While these looping constructs can come in handy from time to time,
         it's often better to use a more functional approach for looping
         such as the functions you learned about in the List module. *)

//---------------------------------------------------------------

//---------------------------------------------------------------
// More About Funtions
//
// You've already learned a little about funcitons in F#, but
// since F# is a functional language, there are more tricks
// to learn!
//---------------------------------------------------------------

// ---- defining lambdas ----------------------------------------

let colors = ["maize"; "blue"]

let echo = 
    colors
    |> List.map (fun x -> x + " " + x)

AssertEquality echo __

(* The fun keyword allows you to create a function inline without giving
   it a name. These functions are known as anonymous functions, lambdas,
   or lambda functions. *)

//---------------------------------------------------------------

// ---- functions that return functions  ------------------------

(* A neat functional programming trick is to create functions that 
   return other functions. This leads to some interesting behaviors. *)
let add x =
    (fun y -> x + y)

(* F#'s lightweight syntax allows you to call both functions as if there
   was only one *)
let simpleResult = add 2 4
AssertEquality simpleResult __

(* ...but you can also pass only one argument at a time to create
   residual functions. This technique is known as partial appliction. *)
let addTen = add 10
let fancyResult = addTen 14

AssertEquality fancyResult __

//NOTE: Functions written in this style are said to be curried.

//---------------------------------------------------------------

// ---- automatic currying --------------------------------------

(* The above technique is common enough that F# actually supports this
   by default. In other words, functions are automatically curried. *)
let add x y = 
    x + y

let addSeven = add 7
let unluckyNumber = addSeven 6
let luckyNumber = addSeven 0

AssertEquality unluckyNumber __
AssertEquality luckyNumber __

//---------------------------------------------------------------

// ---- non curried functions -----------------------------------

(* You should stick to the auto-curried function syntax most of the 
   time. However, you can also write functions in an uncurried form to
   make them easier to use from languages like C# where currying is not 
   as commonly used. *)

let add(x, y) =
    x + y

(* NOTE: "add 5" will not compile now. You have to pass both arguments 
         at once *)

let result = add(5, 40)

AssertEquality result __

(* THINK ABOUT IT: You learned earlier that functions with multiple 
                   return values are really just functions that return
                   tuples. Do functions defined in the uncurried form
                   really accept more than one argument at a time? *)

//---------------------------------------------------------------

//---------------------------------------------------------------
// Apply Your Knowledge!
//
// Below is a list containing comma separated data about 
// Microsoft's stock prices during March of 2012. Without
// modifying the list, programatically find the day with the
// greatest variance between the opening and closing price.
//
// The following functions may be of use:
// 
// abs - takes the absolute value of an arguement
// 
// System.Double.Parse - converts a string argument into a 
//                       numerical value.
//
// The following function will convert a comma separated string
// into an array of the column values.
//                       
// let splitCommas (x:string) =
//     x.Split([|','|])
//---------------------------------------------------------------
    
let stockData =
    [ "Date,Open,High,Low,Close,Volume,Adj Close";
      "2012-03-30,32.40,32.41,32.04,32.26,31749400,32.26";
      "2012-03-29,32.06,32.19,31.81,32.12,37038500,32.12";
      "2012-03-28,32.52,32.70,32.04,32.19,41344800,32.19";
      "2012-03-27,32.65,32.70,32.40,32.52,36274900,32.52";
      "2012-03-26,32.19,32.61,32.15,32.59,36758300,32.59";
      "2012-03-23,32.10,32.11,31.72,32.01,35912200,32.01";
      "2012-03-22,31.81,32.09,31.79,32.00,31749500,32.00";
      "2012-03-21,31.96,32.15,31.82,31.91,37928600,31.91";
      "2012-03-20,32.10,32.15,31.74,31.99,41566800,31.99";
      "2012-03-19,32.54,32.61,32.15,32.20,44789200,32.20";
      "2012-03-16,32.91,32.95,32.50,32.60,65626400,32.60";
      "2012-03-15,32.79,32.94,32.58,32.85,49068300,32.85";
      "2012-03-14,32.53,32.88,32.49,32.77,41986900,32.77";
      "2012-03-13,32.24,32.69,32.15,32.67,48951700,32.67";
      "2012-03-12,31.97,32.20,31.82,32.04,34073600,32.04";
      "2012-03-09,32.10,32.16,31.92,31.99,34628400,31.99";
      "2012-03-08,32.04,32.21,31.90,32.01,36747400,32.01";
      "2012-03-07,31.67,31.92,31.53,31.84,34340400,31.84";
      "2012-03-06,31.54,31.98,31.49,31.56,51932900,31.56";
      "2012-03-05,32.01,32.05,31.62,31.80,45240000,31.80";
      "2012-03-02,32.31,32.44,32.00,32.08,47314200,32.08";
      "2012-03-01,31.93,32.39,31.85,32.29,77344100,32.29";
      "2012-02-29,31.89,32.00,31.61,31.74,59323600,31.74"; ]

//start your program here

let result =  __ //and put your result here to check your work

AssertEquality "2012-3-13" result


// ---------------------------------------------------------------
// Record Types
// 
// In F#, Record Types are lightweight objects that are used to
// bundle bits of data together as properties on an object and 
// give those properties meaningful names.
// ---------------------------------------------------------------

// ---- records have properties ----------------------------------

type Character = {
    Name: string
    Occupation: string
}

let mario = { Name = "Mario"; Occupation = "Plumber"; }

AssertEquality mario.Name __
AssertEquality mario.Occupation __

// ---------------------------------------------------------------

// ---- creating from an existing record -------------------------

let mario = { Name = "Mario"; Occupation = "Plumber"; }
let luigi = { mario with Name = "Luigi"; }

AssertEquality mario.Name __
AssertEquality mario.Occupation __

AssertEquality luigi.Name __
AssertEquality luigi.Occupation __

// ---------------------------------------------------------------

// ---- comparing records ----------------------------------------

let greenKoopa = { Name = "Koopa"; Occupation = "Soldier"; }
let bowser = { Name = "Bowser"; Occupation = "Kidnapper"; }
let redKoopa = { Name = "Koopa"; Occupation = "Soldier"; }

let koopaComparison =
     if greenKoopa = redKoopa then
         "all the koopas are pretty much the same"
     else
         "maybe one can fly"

let bowserComparison = 
    if bowser = greenKoopa then
        "the king is a pawn"
    else
        "he is still kind of a koopa"

AssertEquality koopaComparison __
AssertEquality bowserComparison __

// ---------------------------------------------------------------

// ---- you can pattern match against records --------------------

let mario = { Name = "Mario"; Occupation = "Plumber"; }
let luigi = { Name = "Luigi"; Occupation = "Plumber"; }
let bowser = { Name = "Bowser"; Occupation = "Kidnapper"; }

let determineSide character =
    match character with
    | { Occupation = "Plumber" } -> "good guy"
    | _ -> "bad guy"

AssertEquality (determineSide mario) __
AssertEquality (determineSide luigi) __
AssertEquality (determineSide bowser) __

// ---------------------------------------------------------------

// ---------------------------------------------------------------
// Option Types
//
// Option Types are used to represent calculations that may or
// may not return a value. You may be used to using null for this
// in other languages. However, using option types instead of nulls
// has subtle but far reaching benefits.
// ---------------------------------------------------------------

// ---- option types might contain a value... --------------------

let someValue = Some 10

AssertEquality someValue.IsSome __
AssertEquality someValue.IsNone __
AssertEquality someValue.Value __

// ---------------------------------------------------------------

// ---- ...but they might not ------------------------------------
let noValue = None

AssertEquality noValue.IsSome __
AssertEquality noValue.IsNone __
AssertThrows<FILL_IN_THE_EXCEPTION> (fun () -> noValue.Value)

// ---------------------------------------------------------------

// ---- using option types with pattern matching -----------------

type Game = {
    Name: string
    Platform: string
    Score: int option
}

let chronoTrigger = { Name = "Chrono Trigger"; Platform = "SNES"; Score = Some 5 }
let halo = { Name = "Halo"; Platform = "Xbox"; Score = None }

let translate score =
    match score with
    | 5 -> "Great"
    | 4 -> "Good"
    | 3 -> "Decent"
    | 2 -> "Bad"
    | 1 -> "Awful"
    | _ -> "Unknown"

let getScore game =
    match game.Score with
    | Some score -> translate score
    | None -> "Unknown"

AssertEquality (getScore chronoTrigger) __
AssertEquality (getScore halo) __

// ---------------------------------------------------------------

// ---- projecting values from option types ----------------------

let chronoTrigger = { Name = "Chrono Trigger"; Platform = "SNES"; Score = Some 5 }
let gta = { Name = "Halo"; Platform = "Xbox"; Score = None }

let decideOn game =
    game.Score
    |> Option.map (fun score -> if score > 3 then "play it" else "don't play")

//HINT: look at the return type of the decide on function
AssertEquality (decideOn chronoTrigger) __
AssertEquality (decideOn gta) __

// ---------------------------------------------------------------

// ---------------------------------------------------------------
// Discriminated Unions
//
// Discriminated Unions are used to represent data types that have
// a discrete set of possible states.
// ---------------------------------------------------------------

// ---- descriminated unions capture a set of options ------------

type Condiment =
    | Mustard
    | Ketchup
    | Relish
    | Vinegar

let toColor condiment = 
    match condiment with
    | Mustard -> "yellow"
    | Ketchup -> "red"
    | Relish -> "green"
    | Vinegar -> "brownish?"

let choice = Mustard

AssertEquality (toColor choice) __

(* TRY IT: What happens if you remove a case from the above pattern 
           match? *)

// ---------------------------------------------------------------

// ---- descriminated union cases can have types -----------------

type Favorite =
    | Bourbon of string
    | Number of int

let saySomethingAboutYourFavorite favorite =
    match favorite with
    | Number 7 -> "me too!"
    | Bourbon "Bookers" -> "me too!"
    | Bourbon b -> "I prefer Bookers to " + b
    | Number _ -> "I'm partial to 7"

let bourbonResult = saySomethingAboutYourFavorite <| Bourbon "Maker's Mark"
let numberResult = saySomethingAboutYourFavorite <| Number 7

AssertEquality bourbonResult __
AssertEquality numberResult __

// ---------------------------------------------------------------


//---------------------------------------------------------------
// Modules
//
// Modules are used to group funcitons, values, and types. 
// They're similar to .NET namespaces, but they have slightly 
// different semantics as you'll see below.
//---------------------------------------------------------------

// ---- modules can contain values and types --------------------

module MushroomKingdom =
    type Power =
        | Mushroom
        | Star
        | FireFlower
        
    type Character = {
        Name: string
        Occupation: string
        Power: Power option
    }

    let Mario = { Name = "Mario"; Occupation = "Plumber"; Power = None}

    let powerUp character =
        { character with Power = Some Mushroom }


AssertEquality MushroomKingdom.Mario.Name __
AssertEquality MushroomKingdom.Mario.Occupation __

let moduleType = MushroomKingdom.Mario.GetType()
AssertEquality moduleType typeof<FILL_ME_IN>

//---------------------------------------------------------------

// ---- modules can contain functions ---------------------------

let superMario = MushroomKingdom.powerUp MushroomKingdom.Mario

AssertEquality superMario.Power __

(* NOTE: In previous sections, you've seen modules like List and Option that 
         contain useful functions for dealing with List types and Option types
         respectively. *)

//---------------------------------------------------------------

// ---- opened modules ------------------------------------------
open MushroomKingdom

let OpenedModulesBringTheirContentsInScope() = 
    AssertEquality Mario.Name __
    AssertEquality Mario.Occupation __

//---------------------------------------------------------------

//---------------------------------------------------------------
// Classes
//
// As a full fledged Object Oriented language, F# allows you to
// create traditional classes to contain data and methods.
//---------------------------------------------------------------

// ---- classes can have properties -----------------------------

type Zombie() =
    member this.FavoriteFood = "brains"

    member this.Eat food =
        match food with
        | "brains" -> "mmmmmmmmmmmmmmm"
        | _ -> "grrrrrrrr"

let zombie = new Zombie()

AssertEquality zombie.FavoriteFood __

//---------------------------------------------------------------

// ---- classes can have methods --------------------------------

let zombie = new Zombie()

let result = zombie.Eat "brains"
AssertEquality result __

//---------------------------------------------------------------
 
// ---- classes can have constructors ---------------------------

type Person(name:string) =
    member this.Speak() =
        "Hi my name is " + name

    
let person = new Person("Shaun")

let result = person.Speak()
AssertEquality result __

//---------------------------------------------------------------

// ---- classes can have let bindings in them -------------------

type Zombie2() =
    let favoriteFood = "brains"

    member this.Eat food =
        if food = favoriteFood then "mmmmmmmmmmmmmmm" else "grrrrrrrr"

let zombie = new Zombie2()

let result = zombie.Eat "chicken"
AssertEquality result __

(* TRY IT: Can you access the let bound value Zombie2.favoriteFood
           outside of the class definition? *)

//---------------------------------------------------------------

// ---- classes can have read write properties ------------------

type Person2(name:string) =
    let mutable internalName = name

    member this.Name
        with get() = internalName
        and set(value) = internalName <- value

    member this.Speak() =
        "Hi my name is " + this.Name

let person = new Person2("Shaun")

let firstPhrase = person.Speak()
AssertEquality firstPhrase __

person.Name <- "Shaun of the Dead"
let secondPhrase = person.Speak()
AssertEquality secondPhrase __

//---------------------------------------------------------------