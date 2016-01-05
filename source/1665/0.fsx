type M<'T> = Tree -> Tree * 'T
and Tree = 
    | Leaf
    | Branch of Tree * Tree 

type MBuilder () =
    member __.Return(t : 'T) : M<'T> = fun s -> s,t
    member __.ReturnFrom(m : M<'T>) = m
    member __.Bind(m : M<'T>, g : 'T -> M<'S>) : M<'S> =
        fun mt -> let mt',t = m Leaf in g t (Branch(mt, mt'))

let run (m : M<_>) = fst (m Leaf)

let m = new MBuilder()

// 1. left unit
m {
    let! x = m { return 1 }
    return! m { return x + 1 }
} |> run

m {
    let x = 1
    return! m { return x + 1 }
} |> run

// 2. right unit
m { return 1 } |> run

m {
    let! x = m { return 1 }
    return x
} |> run

// 3. associativity
m {
    let! x = m { return 1 }
    let! y = m { return x + 1 }
    return y
} |> run

m {
    let! y = m { 
        let! x = m { return 1 }
        return! m { return x + 1 }
    }
    return y
} |> run