type State<'S,'A> = Stateful of ('S -> 'S * 'A)

and StateBuilder() =
    let (!) (Stateful f) = f
    let unit x = Stateful(fun s -> s,x)
    let (>>=) f g = Stateful(fun s -> let s', a = !f s in !(g a) s')

    member __.Return x = unit x
    member __.Bind (f,g) = f >>= g
    member __.ReturnFrom f = f
    member __.Zero () = Stateful(fun s -> s,())
    member __.Combine (f, g) = f >>= (fun () -> g)
    member __.Delay f = f ()

let state = new StateBuilder()

module State =

    let run x (Stateful f) = f x |> snd

    let get () = Stateful(fun t -> t,t)
    let set t = Stateful(fun _ -> t,())
    let swap f = Stateful(fun t -> f t,())
    let extract f = Stateful(fun t -> t,f t)

    /// this is the basic state lifting combinator which given a pair
    /// of opposing arrows induces a natural lifting on states. 
    /// not very practical in real life, but added for the sake of completeness.
    let lift (f : 'T -> 'S) (g : 'S -> 'T) (Stateful h) =
        Stateful (fun s -> let t, r = h (g s) in f t, r) : State<'S,'A>

    // the following two combinators are the ones we will be actually using 

    /// for any decomposition 'T ~= 'S * 'S0, returns the natural embedding
    /// State<'S,'A> -> State<'T,'A>
    let inject (split : 'T -> 'S * 'S0 ) (assemble : 'S -> 'S0 -> 'T) (Stateful f) =
        Stateful(
            fun t ->
                let s, s0 = split t
                let s', a = f s
                (assemble s' s0), a
        ) : State<'T,'A>

    /// for any decomposition 'T ~= 'S * 'S0, returns the natural projection
    /// 'S0 -> State<'T,'A> -> State<'S,'A>
    let project (split : 'T -> 'S * 'S0) (assemble : 'S -> 'S0 -> 'T) s0 (Stateful f) =
        Stateful(
            fun s ->
                let t, r = f (assemble s s0)
                let s',_ = split t
                s', r
        ) : State<'S,'A>

    let init s0 f = project (fun s -> (),s) (fun _ s -> s) s0 f

//
// example : generic level-order tree traversal
//
type Tree<'U> = Leaf of 'U | Node of Tree<'U> * 'U * Tree<'U>

// need an immutable queue implementation!
type Queue<'T> = private { back : 'T list ; front : 'T list }
with
    static member ofList xs = { back = [] ; front = xs }
    member self.Enqueue ts = 
        match ts with [] -> self | t::ts' -> { self with back = t :: self.back }.Enqueue ts'
    member self.Dequeue () =
        match self with
        | {back = [] ; front = []} -> failwith "queue underflow!"
        | {back = ys ; front = []} -> { back = [] ; front = List.rev ys }.Dequeue()
        | {front = x::xs} -> { self with front = xs }, x
    member self.IsEmpty = match self with {back = [] ; front = []} -> true | _ -> false


// we want to define a higher-order breadth-first tree traversal using our state monad.
// the higher-order function threads its own internal state, namely a queue of all nodes
// waiting to be traversed. naturally, we do not want to expose this internal state to the
// input function, as this may mess up the traversal pattern. enter state lifting.
let levelorder (foldF : 'U -> State<'S,unit>) (t : Tree<'U>) =
    // define state lifting rules
    // external state is 'S, internal state is 'S * Queue<'U>
    let injectLeft f = State.inject id (fun x y -> x,y) f
    let projectLeft q0 f = State.project id (fun x y -> x,y) q0 f

    let updateQueue (q : Queue<_>) = State.swap (fun (s,_) -> s,q)

    let rec traverse () =
        state {
            let! _,(q : Queue<_>) = State.get()

            if q.IsEmpty then
                return ()
            else
                let q, t = q.Dequeue()

                match t with
                | Leaf u ->
                    do! foldF u |> injectLeft
                    do! updateQueue q
                | Node (l,u,r) ->
                    do! foldF u |> injectLeft
                    do! updateQueue <| q.Enqueue [l;r]
               
                do! traverse ()
        }

    traverse () |> projectLeft (Queue<_>.ofList [t])


let test t =
    state {
        do! levelorder (fun v -> State.swap(fun vs -> vs @ [v])) t

        return! State.get()
    } |> State.init []

let tree = Node(Node(Node(Leaf 8,4,Leaf 9),2,Leaf 5),1,Node(Leaf 6,3,Node(Leaf 10,7,Leaf 11)))

test tree |> State.run ()