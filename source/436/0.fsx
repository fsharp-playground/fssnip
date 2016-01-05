// [snippet:Implementation]
module Seq = 
  /// Reduces input sequence by splitting it into two halves,
  /// reducing each half separately and then aggregating the results 
  /// using the given function. This means that the values are 
  /// aggregated into a ballanced tree, which can save stack space.
  let reduceBallanced f input =
    // Convert the input to an array (must be finite)
    let arr = input |> Array.ofSeq
    let rec reduce s t =
      if s + 1 >= t then arr.[s]
      else 
        // Aggregate two halves of the input separately
        let m = (s + t) / 2
        f (reduce s m) (reduce m t)
    reduce 0 arr.Length
// [/snippet]

// [snippet:Example using binary trees]

/// Simple tree type with data in leaves
type Tree = 
  | Node of Tree * Tree
  | Leaf of int
  /// Returns the size of a tree
  member x.Size = 
    match x with 
    | Leaf _ -> 1 
    | Node(t1, t2) -> 1 + (max t1.Size t2.Size)

// Aggregate 1000 leaves in a tree
let inputs = [ for n in 0 .. 1000 -> Leaf n]

// 'reduceBallanced' creates tree with size 11
let t1 = inputs |> Seq.reduceBallanced (fun a b -> Node(a, b))
t1.Size

// Ordinary 'reduce' creates tree with size 1001
let t2 = inputs |> Seq.reduce (fun a b -> Node(a, b))
t2.Size
// [/snippet]