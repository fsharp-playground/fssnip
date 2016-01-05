type YourType () =
    interface IEnumerable<'T> with
        member r.GetEnumerator () =
            let s = seq {}
            s.GetEnumerator()
    interface IEnumerable with
        member r.GetEnumerator () =
            (r:>IEnumerable<'T>).GetEnumerator():>IEnumerator
