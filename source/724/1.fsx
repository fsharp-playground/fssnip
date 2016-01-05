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

// ---- using parenthesis to control the order of operation -----

let add x y =
    x + y

let result = add (add 5 8) (add 1 1)

AssertEquality result __

(* TRY IT: What happens if you remove the parensthesis?*)

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

let items = ("apple", "dog", "Mustang")

let fruit, animal, car = items

AssertEquality fruit __
AssertEquality animal __
AssertEquality car __

(* Breaking apart tuples in this way uses a F# feature called 
   pattern matching. Pattern matching is another key concept in 
   F#, and you'll see more examples of it later on. *)

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
    
(* If you've worked with other programming languages, you might
   be surprised to find out that if statements in F# return
   values. *)
   
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

// ---- branching with tuples and pattern matching --------------
    
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
// Containers
//
// Containers are very important building blocks that you'll use 
// in F# programming. They are used to group arbitrarily large 
// sequences of values. It's very common to store values in a 
// container and perform operations across each value in the 
// sequence.
//---------------------------------------------------------------

//---------------------------------------------------------------
// Lists
//---------------------------------------------------------------

// ---- creating lists ------------------------------------------

let list = ["apple"; "pear"; "grape"; "peach"]

AssertEquality list.Head __
AssertEquality list.Tail __
AssertEquality list.Length __

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

//---------------------------------------------------------------
// Arrays
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

//---------------------------------------------------------------
// Operations on Sequences
//
// The following sections describe the Seq module which contains
// a bunch of functions for working with data inside of a
// collection. Note that any operations in the Seq module work
// on both arrays and lists.
//---------------------------------------------------------------
   
// ---- transforming collections with map -----------------------

let square x =
    x * x

let original = [0..5]
let result = Seq.map square original

AssertEquality original __
AssertEquality result __

//---------------------------------------------------------------
    
// ---- filtering collections -----------------------------------
 
let isEven x =
    x % 2 = 0

let original = [|0..5|]
let result = Seq.filter isEven original

AssertEquality original __
AssertEquality result __

//---------------------------------------------------------------

// ---- skipping elements ---------------------------------------

let original = [0..5]
let result = Seq.skip 2 original

AssertEquality result __

//---------------------------------------------------------------

// ---- finding the max -----------------------------------------

let values = [11; 20; 4; 2; 3]
let result = Seq.max values

AssertEquality result __

//---------------------------------------------------------------

// ---- finding the maximum using a condition -------------------

let getNameLength (name:string) =
    name.Length

let names = [| "Harry"; "Lloyd"; "Nicholas"; "Mary"; "Joe"; |]
let result = Seq.maxBy getNameLength names 

AssertEquality result __


(* Note: There are many other useful methods in the Seq module. Check them
   via intellisense in Visual Studio by typing '.' after Seq, or online at
   http://msdn.microsoft.com/en-us/library/ee353738.aspx *)

//---------------------------------------------------------------

//---------------------------------------------------------------
// Pipelining
//
// Now that you've seen a few operations for working with lists, 
// you can  combine them to do more interesting things
//---------------------------------------------------------------


// ---- squaring even numbers with separate statements ----------

let square x =
    x * x

let isEven x =
    x % 2 = 0

(* One way to combine the operations is by using separate statements.
   However, this is can be clumsy since you have to name each result. *)

let numbers = [0..5]

let evens = List.filter isEven numbers
let result = List.map square evens

AssertEquality result __

//---------------------------------------------------------------

// ---- squaring even numbers with parens -----------------------

(* You can avoid this problem by using parens to pass the result of one
   funciton to another. This can be difficult to read since you have to 
   start from the innermost function and work your way out. *)

let numbers = [0..5]

let result = List.map square (List.filter isEven numbers)

AssertEquality result __

//---------------------------------------------------------------

// ---- squaring even numbers with the pipeline operator --------

(* In F#, you can use the pipeline operator to get the benefit of the 
   parens style with the readablity of the statement style. *)

let result =
    [0..5]
    |> List.filter isEven
    |> List.map square

AssertEquality result __

//---------------------------------------------------------------

// ---- using lambdas -------------------------------------------

let colors = ["maize"; "blue"]

let echo = 
    colors
    |> List.map (fun x -> x + " " + x)

AssertEquality echo __

(* The fun keyword allows you to create a function inline without giving
   it a name. These functions are known as anonymous functions, lambdas,
   or lambda functions. *)

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

let result =  __

AssertEquality "2012-3-13" result