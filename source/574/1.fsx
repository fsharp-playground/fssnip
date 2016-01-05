open System
open System.Collections.Generic

type Lifetime = Singleton | Transient

type private Constructor =
    | Reflected of Type
    | Factory of (unit -> obj)

type Container () =
    let catalog = Dictionary<Type, Constructor * Lifetime>()
    let singletons = Dictionary<Type,obj>()
    let rec resolve t =
        match catalog.TryGetValue(t) with
        | true, (Factory f, lifetime)-> 
            obtain t (fun () -> f() |> Some) lifetime
        | true, (Reflected u, lifetime) ->
            obtain t (fun () -> construct u) lifetime
        | false, _ -> 
            obtain t (fun () -> construct t) Singleton 
    and obtain t f lifetime =
        match singletons.TryGetValue t with
        | true, value -> Some value
        | false, _ ->
            let result = f()
            result |> Option.iter (fun value -> store lifetime t value)
            result
    and store lifetime t value =
        match lifetime with
        | Singleton -> singletons.Add(t,value)
        | Transient -> ()
    and construct t =
        t.GetConstructors() |> Array.tryPick (fun ctr ->
            let ps = ctr.GetParameters()
            let args = ps |> Array.map (fun p -> resolve p.ParameterType)
            match args |> Array.forall Option.isSome with
            | true -> args |> (Array.choose id) |> ctr.Invoke |> Some
            | false -> None
        )
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (concreteType:Type, lifetime:Lifetime) =
        catalog.Add(typeof<'TAbstract>, (Reflected(concreteType),lifetime))
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (concreteType:Type) = 
        container.Register<'TAbstract>(concreteType,Singleton)
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (f:unit->'TAbstract, lifetime:Lifetime) = 
        catalog.Add(typeof<'TAbstract>, (Factory(f >> box), lifetime))
    member container.Register(f:unit->'TAbstract) = 
        container.Register(f, Singleton)
    member container.Resolve<'TAbstract when 'TAbstract : not struct>() =
        let t = typeof<'TAbstract>
        match resolve t with
        | Some value -> value :?> 'TAbstract
        | None -> sprintf "Failed to resolve %A" t |> invalidOp
    member container.Release(instance:obj) =
        singletons 
        |> Seq.filter (fun pair -> pair.Value = instance)
        |> Seq.toList
        |> List.iter (fun pair -> singletons.Remove(pair.Key) |> ignore)

module Usage =
    
    let container = new Container()
    
    type A () =
        do  printfn "A"

    type B () =
        do  printfn "B"

    type C (a:A, b:B) =
        do  printfn "C"

    container.Register((fun () -> printfn "B lambda"; B()), Transient)
    container.Register<C>(typeof<C>)
    let c = container.Resolve<C>()
    let c' = container.Resolve<C>()
    container.Release(c)
    let c'' = container.Resolve<C>()

    type ICalculate =
        abstract member Incr : int -> int

    type Calculator () =
        interface ICalculate with
            member this.Incr(x:int) = x + 1
    
    container.Register<ICalculate>(typeof<Calculator>)

    let calc = container.Resolve<ICalculate>()
    printfn "%d" (calc.Incr 1)
    Console.ReadLine() |> ignore