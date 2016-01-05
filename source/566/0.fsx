/// Represents a non-deterministic computation that
/// returns some calculated value of 'T
type Nondet<'T> = ND of seq<'T>

/// Simple computation builder for non-deterministic
/// computations. You can use 'require' to specify
/// assertions and 'choose' to generate possibilities.
type NondetBuilder() =
  member x.Bind(ND v, f) = ND (Seq.collect (fun v -> let (ND s) = f v in s) v)
  member x.Return(v) = ND (seq { yield v })

/// Computation builder for non-deterministic computations
let nondet = NondetBuilder()

/// Specifies an assertion on non-deterministic computation
/// (the computation will only succeed if 'b' is true)
let require b = 
  ND (if not b then Seq.empty else seq { yield () })

/// Returns a nondeterministic computation that may 
/// return any of the specified values
let choose opts = ND opts

/// Find some solution of the non-deterministic computation
/// (may run for really long time or fail!)
let solve (ND c) = Seq.head c

// Sample non-deterministic computation. Find 'a' and 'b'
// from 1 to 50 such that 'a * b = 91'
nondet {
  let! a = choose [ 1 .. 50 ]
  let! b = choose [ 1 .. 50 ]
  let c = a * b
  do! require (c = 91) 
  return (a, b) }
|> solve