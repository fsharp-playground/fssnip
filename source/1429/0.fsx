open System

// Step 100 takes about 3 sec on my machine
// so we can iterate over all flaot32 values
// in something like 5 minutes :-)
let step = 100 

// This can quite likely be optimized too :-)
for v in Int32.MinValue .. step .. Int32.MaxValue do
  let f = BitConverter.ToSingle(BitConverter.GetBytes(v), 0)
  // If they are both NaN, then that's okay, but since
  // NaN <> NaN, we need to check that case explicitly
  if Single.IsNaN(f) <> Single.IsNaN(1.0f * f) && 
     f <> 1.0f * f then 
    failwithf "Failed for: %f" f
