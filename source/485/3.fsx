// [snippet: delimited continuation monad]
module ShiftResetGenuine

type DCont<'a,'b,'tau> = ('tau -> 'a) -> 'b

type DContBuilder() =
    member this.Return(x):DCont<_,_,_> =
        fun k -> k x
    member this.Bind(f:DCont<_,_,_>, h:_ -> DCont<_,_,_>):DCont<_,_,_> =
        fun k -> f <| fun s -> h s k

let dcont = DContBuilder()

let ret = dcont.Return
let run (f:DCont<_,_,_>) = f id
let reset (f:DCont<_,_,_>):DCont<_,_,_> =
    fun k -> k (f id)
let shift (f:(_ -> DCont<_,_,_>) -> DCont<_,_,_>):DCont<_,_,_> =
    fun k -> f (fun tau -> ret <| k tau) id
// [/snippet]


// [snippet: example 1]
#nowarn "40"

let inversion walker collection =
    let rec c =
        ref <| fun () ->
            reset <| dcont {
                let! _ = walker (fun e -> 
                    shift <| fun k -> 
                    c := fun () -> k e
                    ret e) collection
                return failwith "no more elements"
            }
    fun () -> !c ()

// an internal iterator for lists
let rec map f list = dcont {
    match list with
    | []    -> return []
    | x::xs -> let! x' = f x
               let! xs' = map f xs
               return x'::xs'
}

[1..10]
|> map ((+) 1 >> ret)
|> run
|> printfn "%A"

// creating an external iterator from the internal iterator
let invMap = inversion map

let iterator = invMap [1..10]

for i = 1 to 10 do
    iterator()
    |> run
    |> (+) 1
    |> printfn "%A"
// [/snippet]


// [snippet: example 2]
type Tree<'a> = Leaf | Node of 'a * Tree<'a> * Tree<'a>

// an internal iterator for trees
let rec mapTree f tree = dcont {
    match tree with
    | Leaf          -> return Leaf
    | Node(a, l, r) -> let! a' = f a
                       let! l' = mapTree f l
                       let! r' = mapTree f r
                       return Node(a', l', r')
}

let invMapTree = inversion mapTree

let iterator' = invMapTree <| Node(1, Node(2, Node(3, Leaf, Leaf), Leaf), Leaf)

for i = 1 to 3 do
    iterator'()
    |> run
    |> (+) 1
    |> printfn "%A"
// [/snippet]
