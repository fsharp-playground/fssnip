let spliceInto replace (source: 'a seq) n insert = seq {
    // yield the first n - 1 elements
    let i = ref 0
    let enum = source.GetEnumerator()
    while !i < n && enum.MoveNext() do
        yield enum.Current
        incr i

    // skip the nth element if we're in replace mode
    if replace then enum.MoveNext() |> ignore

    // yield the sequence to splice in 
    yield! insert

    // yield the rest of the original sequence
    while enum.MoveNext() do yield enum.Current
    }

// let xs = [1; 2; 3]
// let ys = [4; 5; 6]
// 
// assert (Enumerable.SequenceEqual(spliceInto true xs 0 ys, [4; 5; 6; 2; 3]))
// assert (Enumerable.SequenceEqual(spliceInto false xs 0 ys, [4; 5; 6; 1; 2; 3]))
// 
// assert (Enumerable.SequenceEqual(spliceInto true xs 1 ys, [1; 4; 5; 6; 3]))
// assert (Enumerable.SequenceEqual(spliceInto false xs 1 ys, [1; 4; 5; 6; 2; 3]))
// 
// assert (Enumerable.SequenceEqual(spliceInto true xs 2 ys, [1; 2; 4; 5; 6]))
// assert (Enumerable.SequenceEqual(spliceInto false xs 2 ys, [1; 2; 4; 5; 6; 3]))
// 
// assert (Enumerable.SequenceEqual(spliceInto true xs 3 ys, [1; 2; 3; 4; 5; 6]))
// assert (Enumerable.SequenceEqual(spliceInto false xs 3 ys, [1; 2; 3; 4; 5; 6]))
// 
// assert (Enumerable.SequenceEqual(spliceInto true xs 4 ys, [1; 2; 3; 4; 5; 6]))
// assert (Enumerable.SequenceEqual(spliceInto false xs 4 ys, [1; 2; 3; 4; 5; 6]))
