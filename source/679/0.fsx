type Maybe<'a> (defaultValue : 'a) =
    member m.Bind(x : 'a option, f) = Option.bind f x
    member m.Return(x : 'a) = Some(x)
    member m.Zero() = None
    member m.ReturnFrom(x : 'a option) = x
    member m.Run(x : 'a option) =
        if Option.isSome x then Option.get x
        else defaultValue
   
type 'a Zipper = ('a list)*('a list)
let (head : 'a Zipper -> 'a option) = function
    | (_,[]) -> None
    | (_,h::t) -> Some h
let (right : 'a Zipper -> 'a Zipper option) = function
    | (_,[]) -> None
    | (l,h::t) -> Some (h::l,t)
let (left : 'a Zipper -> 'a Zipper option) = function
    | ([],_) -> None
    | (h::t,r) -> Some (t,h::r)
let mkzip l = ([],l)
    
let maybeZ = new Maybe<int Zipper>(([],[]))
let maybeE = new Maybe<int>(0)

let myz : int Zipper = ([3;2;1],[4;5;6])

let z1 = maybeZ {
    let! z = left myz
    let! z = left z
    let! z = right z
    let! z = right z
    let! z = right z
    return z
}

let z2 = maybeZ {
    let! z = right myz
    let! z = right z
    let! z = right z
    let! z = right z
    let! z = right z
    let! z = right z
    return z
}

let z3 = maybeE {
    return! head z1
}

let z4 = maybeE {
    return! head z2
}

let z5 = maybeE {
    return! head (maybeZ {
        return! left myz
    })
}