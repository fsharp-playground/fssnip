let farey n =
    seq {
        let p = ref 0
        let q = ref 1
        let p' = ref 1
        let q' = ref n
        yield (!p, !q)
        while not (!p = 1 && !q = 1) do
            let c = (!q + n) / !q'
            let p'' = c * !p' - !p
            let q'' = c * !q' - !q
            p := !p'
            q := !q'
            p' := p''
            q' := q''
            yield (!p, !q) }