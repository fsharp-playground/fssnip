// A method with a “normal” (call-by-value) argument is declared like this 
(*
def foo(x: Int) = {
    val a = x
    val b = x
    (a, b)
}
foo({println("hello"); 2 + 2})

//hello
//res0: (Int, Int) = (4,4)
*)

let foo (x : int) =
    let a = x
    let b = x
    a, b
foo (printfn "Hello"; 2 + 2)
//Hello
//val it : int * int = (4, 4)

// A method with a lazy (call-by-name) argument is declared like this 
(*
def foo(x: => Int) = {
    val a = x
    val b = x
    (a, b)
}
foo({println("hello"); 2 + 2})

//hello
//hello
//res0: (Int, Int) = (4,4)
*)

let foo (x : unit -> int) =
    let a = x()
    let b = x()
    a, b
foo <| fun () -> printfn "hello"; 2 + 2
//hello
//hello
//val it : int * int = (4, 4)

// A method that takes a no-argument function (a thunk) is declared like this 
(*
def foo(x:() => Int) = {
    val a = x
    val b = x
    val c = x()
    (a, b, c)
}
foo(() => {println("hello"); 2 + 2})

//hello
//res0: (() => Int, () => Int, Int) = (<function>,<function>,4)

*)

let foo (x : unit -> int) =
    let a = x
    let b = x
    let c = x()
    a, b, c
foo <| fun () -> printfn "hello"; 2 + 2
//hello
//val it : (unit -> int) * (unit -> int) * int =
//  (<fun:it@65-10>, <fun:it@65-10>, 4)

// Lazy values allow you to turn call-by-name lazy arguments into call-by-need lazy arguments. 
(*
//def foo(def x: Int) = {
def foo(x: => Int) = {
    lazy val a = x
    (a, a, a)
}
foo({println("hello"); 2 + 2})

hello
res0: (Int, Int, Int) = (4,4,4)
*)

let foo (x : Lazy<int>) =
    let a = x
    a.Value, a.Value, a.Value
foo <| lazy (printfn "hello"; 2 + 2)
//hello
//val it : int * int * int = (4, 4, 4)
