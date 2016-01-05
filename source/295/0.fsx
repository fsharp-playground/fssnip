// Computation expression for 
// Cartesian products.
// I use lists here, but any type with
// aggregate operations will work 
// (e.g. sequences).
// Notice how tiny it would be without
// all these comments and formatting -- 
// just three lines of code!
type Product () =

  member this.Bind (l,f) =
    // Collect lets you capture the result
    // in a list, but other operations 
    // are possible.  For example, map 
    // builds a hierarchy, and iter will 
    // let you use the results without 
    // aggregating them (return must be 
    // modified as shown below).
    List.collect f l

  member this.Return n = 
    // For collect and map:
    [n]
    // For iter:
    //()


let enumeratedPizzas = 
  Product() {
    // I never met a pizza I didn't like.
    let! x = ["New York";"Chicago"]
    let! y = ["Pepperoni";"Sausage"]
    let! z = ["Cheese";"Double Cheese"]
    // I capture the results in a tuple,
    // but you can do whatever here,
    // depending on aggregation, etc.
    return x,y,z
  }
