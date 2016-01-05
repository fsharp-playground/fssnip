// Let's say that creating FsiEvaluator() fails with mysterious
// error from the F# compiler service. We can catch the exception:

let e = 
  try new FSharp.Literate.FsiEvaluator(); failwith "!" with e -> e

// Get the InnerException, which is the actual error from the compiler
let ae = e.InnerException

// And get the values of the private fields!
let opts = Reflection.BindingFlags.NonPublic|||Reflection.BindingFlags.Instance
[ for p in ae.GetType().GetFields(opts) -> p.Name, p.GetValue(ae) ]

// This might give you some more useful information about the
// error (e.g. for `FileNameNotResolved`, you actually get the file name..)