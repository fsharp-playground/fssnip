let triplewise (source: seq<_>) =
    seq { use e = source.GetEnumerator() 
        if e.MoveNext() then
            let i = ref e.Current
            if e.MoveNext() then
                let j = ref e.Current
                while e.MoveNext() do
                    let k = e.Current 
                    yield (!i, !j, k)
                    i := !j
                    j := k }

// sample
triplewise [1..4] // -> seq [(1, 2, 3); (2, 3, 4)]