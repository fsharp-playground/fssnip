type T = interface end
type L = inherit T
type B<'T1, 'T2 when 'T1 :> T and 'T2 :> T> = inherit T

type M<'C, 'T when 'C :> T> = M of (unit -> 'T)

type MBuilder () =
    member __.Return(t : 'T) : M<L,'T> = M(fun () -> t)
    member __.ReturnFrom(m : M<_,_>) = m
    member __.Bind((M f) : M<'C1,'T>, g : 'T -> M<'C2, 'S>) : M<B<'C1,'C2>, 'S> =
        M(fun () -> let t = f () in let (M sf) = g t in sf ())


let m = new MBuilder()

// 1. left unit
let u1l = m {
    let! x = m { return 1 }
    return! m { return x + 1 }
}

let u1r = m {
    let x = 1
    return! m { return x + 1 }
}

// 2. right unit
let u2l = m { return 1 }

let u2r = m {
    let! x = m { return 1 }
    return x
}

// 3. associativity
let assl = m {
    let! x = m { return 1 }
    let! y = m { return x + 1 }
    return y
}

let assr = m {
    let! y = m { 
        let! x = m { return 1 }
        return! m { return x + 1 }
    }
    return y
}