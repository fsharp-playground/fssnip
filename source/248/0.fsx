open System
open System.Collections.Generic

// [snippet:Definition of imperative computations]
// For more information about this snippet, see the blog post:
// * http://tomasp.net/blog/imperative-i-return.aspx

/// A type that represents imperative computation
/// that runs and may return a result at the end
type Imperative<'T> = unit -> option<'T>

type ImperativeBuilder() = 
  // Creatae computation that returns the given value  
  member x.Return(v) : Imperative<_> = 
    (fun () -> Some(v))
  // Create computation that doesn't return any value
  member x.Zero() = (fun () -> None)

  // Return a computation that will evaluate the provided function  
  // only when the computation is being evaluated
  member x.Delay(f:unit -> Imperative<_>) = 
    (fun () -> f()())
  
  // Combines two delayed computations (that may return 
  // value imperatively using 'return') into one  
  member x.Combine(a, b) = (fun () ->
    // run the first part of the computation
    match a() with 
    // if it returned, we can return the result immediately
    | Some(v) -> Some(v) 
    // otherwise, we need to run the second part
    | _ -> b() )
  
  // Execute the imperative computation 
  // expression given as an argument
  member x.Run(imp) = 
    // run the computation and return the result or 
    // fail when the computation didn't return anything
    match imp() with 
    | Some(v) -> v 
    | None -> failwith "nothing returned!"

  member x.For(inp:seq<_>, f) =
    // Process next element from the sequence
    let rec loop(en:IEnumerator<_>) = 
      // If ther are no more elements, return empty computation
      if not(en.MoveNext()) then x.Zero() else
        // Otherwise call body and combine it with a 
        // computation that continues looping
        x.Combine(f(en.Current), x.Delay(fun () -> loop(en)))
    // Start enumerating from the first element
    loop(inp.GetEnumerator())
    
  member x.While(gd, body) = 
    // Perform one step of the 'looping'
    let rec loop() =
      // If the condition is false, return empty computation
      if not(gd()) then x.Zero() else
        // Otherwise, call body and then loop again
        x.Combine(body, x.Delay(fun () -> loop()))
    loop()

let imperative = new ImperativeBuilder()
// [/snippet]

// [snippet:Basic examples of imperative computations]
// Code following the first 'return' is never executed
let test(b) = imperative {
  if b then 
    return 0
  printfn "after return!"
  return 1 }

// Imperatively returns 'false' if string fails to pass a check
let validateName(arg:string) = imperative {
  // Should be non-empty and should contain space
  if (arg = null) then return false
  let idx = arg.IndexOf(" ")
  if (idx = -1) then return false
    
  // Verify the name and the surname
  let name = arg.Substring(0, idx)
  let surname = arg.Substring(idx + 1, arg.Length - idx - 1)
  if (surname.Length < 1 || name.Length < 1) then return false
  if (Char.IsLower(surname.[0]) || Char.IsLower(name.[0])) then return false

  // Looks like we've got a valid name!
  return true }
//[/snippet]

//[snippet:Imperatively returning from a loop]
let readFirstName() = imperative {
  // Loop until the user enters valid name
  while true do
    let name = Console.ReadLine()
    // If the name is valid, we return it, otherwise
    // we continue looping...
    if (validateName(name)) then return name
    printfn "That's not a valid name! Try again..." }

/// Imperatively returns 'true' as soon as a value 
/// matching the specified predicate is found
let exists f inp = imperative {
    for v in inp do 
      if f(v) then return true
    return false }

[ 1 .. 10 ] |> exists (fun v -> v % 3 = 0)
// [/snippet]