open System.Threading

type Transaction<'T,'A> = T of ('T -> 'T * 'A)

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

    member self.Value with get() : 'T = !refCell
    member self.Swap (f : 'T -> 'T) : unit = swap f
    member self.Commit<'A> (f : Transaction<'T,'A>) : 'A = match f with T f0 -> transact f0
    

    static member get : Transaction<'T,'T> = T (fun t -> t,t)
    static member set : 'T -> Transaction<'T,unit> = fun t -> T (fun _ -> t,())

    static member Create<'T> (x : 'T) = new Atom<'T>(x)



type TransactionBuilder() =
    let (!) = function T f -> f

    member __.Return (x : 'A) : Transaction<'T,'A> = T (fun t -> t,x)
    member __.Bind (f : Transaction<'T,'A> , g : 'A -> Transaction<'T,'B>) : Transaction<'T,'B> =
        T (fun t ->
            let t',x = !f t
            !(g x) t'
        )

let transact = new TransactionBuilder()

// example
type Stack<'T> () =
    let container : Atom<'T list> = Atom.Create []

    member __.Push (x : 'T) =
        transact {
            let! contents = Atom.get

            do! Atom.set <| x :: contents

            return ()
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