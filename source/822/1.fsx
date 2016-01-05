type Mode = Singleton | Factory

type private DependencyContainer<'T> () =
    // this implementation does not account for thread safety
    // a good way to go would be to wrap the container map
    // in an atom structure like the one in http://fssnip.net/bw
    static let container : Map<string option, unit -> 'T> ref = ref Map.empty

    static let t = typeof<'T>

    static let morph mode (f : unit -> 'T) =
        match mode with
        | Singleton ->
            let singleton = lazy(f ())
            fun () -> singleton.Value
        | Factory -> f

    static member Register (mode, factory : unit -> 'T, param) =
        if (!container).ContainsKey param then
            match param with
            | None -> failwithf "IoC : instance of type %s has already been registered" t.Name
            | Some param -> failwithf "IoC : instance of type %s with parameter \"%s\" has already been registered" t.Name param
        else container := (!container).Add(param, morph mode factory)
            

    static member IsRegistered param = (!container).ContainsKey param

    static member TryResolve param =
        match (!container).TryFind param with
        | None -> None
        | Some f ->
            try Some <| f ()
            with e -> 
                match param with
                | None -> failwithf "IoC : factory method for type %s has thrown an exception:\n %s" t.Name <| e.ToString()
                | Some param -> 
                    failwithf "IoC : factory method for type %s with parameter \"%s\" has thrown an exception:\n %s" t.Name param <| e.ToString()

    static member Resolve param =
        match DependencyContainer<'T>.TryResolve param with
        | Some v -> v
        | None ->
            match param with
            | None -> failwithf "IoC : no instace of type %s has been registered" t.Name
            | Some param -> failwithf "IoC : no instance of type %s with parameter \"%s\" has been registered" t.Name param


type IoC =
    static member Register<'T> (mode, factory, ?param) = DependencyContainer<'T>.Register(mode, factory, param)
    static member RegisterValue<'T> (value, ?param) = DependencyContainer<'T>.Register(Factory, (fun () -> value), param)
    static member TryResolve<'T> ?param = DependencyContainer<'T>.TryResolve param
    static member Resolve<'T> ?param = DependencyContainer<'T>.Resolve param
    static member IsRegistered<'T> ?param = DependencyContainer<'T>.IsRegistered param


// example

type IAnimal =
    abstract Name : string

type Cat(name) =
    member __.Purr() = printfn "purrr"
    interface IAnimal with
        member __.Name = name
        
IoC.Register<IAnimal>(Singleton, fun () -> new Cat("Mr. Bungle") :> IAnimal)
IoC.RegisterValue<IAnimal>( { new IAnimal with member __.Name = "Eirik" }, "me")

let cat = IoC.Resolve<IAnimal>() :?> Cat
let me = IoC.Resolve<IAnimal> "me"

cat.Purr()
me.Name

IoC.RegisterValue(42, "magic number")
IoC.Resolve<int> "magic number"