open System.Threading

type Transaction<'State,'Output> = T of ('State -> 'State * 'Output)

type Atom<'T when 'T : not struct>(value : 'T) =
    let refCell = ref value
    
    let rec swap (f : 'T -> 'T) = 
        let currentValue = !refCell
        let result = Interlocked.CompareExchange<'T>(refCell, f currentValue, currentValue)
        if obj.ReferenceEquals(result, currentValue) then ()
        else Thread.SpinWait 20; swap f

    let transact (f : 'T -> 'T * 'A) =
        let output = ref Unchecked.defaultof<'A>
        let f' x = let t,s = f x in output := s ; t
        swap f' ; output.Value

    static member Create<'T> (x : 'T) = new Atom<'T>(x)

    member self.Value with get() : 'T = !refCell
    member self.Swap (f : 'T -> 'T) : unit = swap f
    member self.Commit<'A> (f : Transaction<'T,'A>) : 'A = 
        match f with T f0 -> transact f0 

    static member get : Transaction<'T,'T> = T (fun t -> t,t)
    static member set : 'T -> Transaction<'T,unit> = fun t -> T (fun _ -> t,())

type TransactionBuilder() =
    let (!) = function T f -> f

    member __.Return (x : 'A) : Transaction<'T,'A> = T (fun t -> t,x)
    member __.ReturnFrom (f : Transaction<'T,'A>) = f
    member __.Bind (f : Transaction<'T,'A> , 
                    g : 'A -> Transaction<'T,'B>) : Transaction<'T,'B> =
        T (fun t -> let t',x = !f t in !(g x) t')

let transact = new TransactionBuilder()

// example : thread safe stack
type Stack<'T> () =
    let container : Atom<'T list> = Atom.Create []

    member __.Push (x : 'T) =
        transact {
            let! contents = Atom.get

            return! Atom.set <| x :: contents
        } |> container.Commit

    member __.Pop () =
        transact {
            let! contents = Atom.get

            match contents with
            | [] -> return failwith "stack is empty!"
            | head :: tail ->
                do! Atom.set tail
                return head
        } |> container.Commit

    member __.Flush () =
        transact {
            let! contents = Atom.get

            do! Atom.set []

            return contents
        } |> container.Commit