// [snippet:Background]
open System

// Let's say we have an interface representing vectors
// (in reality this would have quite a few members)
type IVector =
  abstract Data : seq<float>
  abstract GetValue : int -> float

// Now, we have a concrete vector implementation using an array
type ArrayVector(data:float[]) = 
  interface IVector with
    member x.Data = data |> Seq.ofArray
    member x.GetValue(i) = data.[i]
// [/snippet]

// [snippet:Currently]
// And we want to create a "lazy" vector that loads the data
// from database. It has range of values and other operations
// can check for 'DatabaseVector' and instead of actually working
// with in-memory data, they can just adjust min/max date...
//
// When we actually access the data of the vector, this needs
// to load the data from the database and redirect all operations
// to an actual in-memory vector
type DatabaseVector(min:DateTime, max:DateTime) = 
  let vector = Lazy.Create(fun () ->
    let count = int (max - min).TotalDays 
    let data = [| for i in 0 .. count - 1 -> 3.0 |]
    ArrayVector(data) :> IVector)

  // Now we have to implement the interface by writing
  // all the members and just making calls to 'vector' :-((
  interface IVector with
    member x.Data = vector.Value.Data
    member x.GetValue(i) = vector.Value.GetValue(i)
// [/snippet]

// [snippet:Suggestion]
type DatabaseVector(min:DateTime, max:DateTime) = 
  let vector = Lazy.Create(fun () ->
    let count = int (max - min).TotalDays 
    let data = [| for i in 0 .. count - 1 -> 3.0 |]
    ArrayVector(data) :> IVector)

  // I would like to be able to say "implement this interface"
  // by delegating all the operations to the object returned by 
  // an expression <expr>. In this example, the expression is just
  // 'vector.Value' (but you can imagine other functions to implement
  // common interfaces like IDisposable, etc...)
  interface IVector = vector.Value
// [/snippet]