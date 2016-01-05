open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection

type Lifetime = Singleton | Transient

type AbstractType = Type
type ConcreteType = Type

type private Constructor =
    | Reflected of ConcreteType
    | Factory of (unit -> obj)
 
let private (|FunType|_|) t =
    if FSharpType.IsFunction t 
    then FSharpType.GetFunctionElements t |> Some
    else None
     
let private (|FuncType|_|) (t:Type) =
    if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Func<_>> 
    then t.GetGenericArguments().[0] |> Some
    else None

type Container () as this =
    let catalog = Dictionary<AbstractType, Constructor * Lifetime>()
    let singletons = Dictionary<ConcreteType,obj>()
    let rec tryResolve t =
        match catalog.TryGetValue t with
        | true, (Factory f, lifetime) -> 
            tryObtain t (fun () -> f() |> Some) lifetime
        | true, (Reflected u, lifetime) -> 
            tryObtain u (fun () -> tryConstruct u) lifetime
        | false, _ -> 
            tryObtain t (fun () -> tryConstruct t) Singleton 
    and tryObtain t f lifetime =
        match singletons.TryGetValue t with
        | true, value -> Some value
        | false, _ ->
            let result = f()
            result |> Option.iter (fun value -> store t value lifetime)
            result
    and store t value = function
        | Singleton -> singletons.Add(t,value)
        | Transient -> ()
    and tryConstruct t =
        t.GetConstructors() 
        |> Array.sortBy (fun c -> c.GetParameters().Length)
        |> Array.tryPick (fun ctr ->
            let ps = ctr.GetParameters()
            let args = ps |> Array.map (fun p -> tryResolveArgument p.ParameterType)             
            match args |> Array.forall Option.isSome with
            | true -> args |> (Array.choose id) |> ctr.Invoke |> Some
            | false -> None
        )
    and tryResolveArgument t =
        match t with         
        | FunType(arg,result) when arg = typeof<unit> ->
            FSharpValue.MakeFunction(t,fun args -> this.Resolve(result)) |> Some
        | FuncType result ->                                
            let mi = typeof<Container>.GetMethod("Resolve",[||]).MakeGenericMethod(result)
            Delegate.CreateDelegate(t, this, mi) |> box |> Some
        | t -> tryResolve t
    static let current = Container()
    static member Current = current
    member container.Register(abstractTypes:AbstractType seq, concreteType:ConcreteType) =    
        for t in abstractTypes do catalog.Add(t, (Reflected concreteType, Singleton))
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (concreteType:ConcreteType, lifetime:Lifetime) =
        catalog.Add(typeof<'TAbstract>, (Reflected(concreteType), lifetime))
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (concreteType:ConcreteType) = 
        container.Register<'TAbstract>(concreteType, Singleton)
#if CSHARP
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (f:Func<'TAbstract>) = 
        catalog.Add(typeof<'TAbstract>, (Factory(fun () -> f.Invoke() |> box), Singleton))   
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (f:Func<'TAbstract>, lifetime:Lifetime) = 
        catalog.Add(typeof<'TAbstract>, (Factory(fun () -> f.Invoke() |> box), lifetime))   
#else
    member container.Register<'TAbstract when 'TAbstract : not struct>(f:unit->'TAbstract) =   
        catalog.Add(typeof<'TAbstract>, (Factory(f >> box), Singleton))
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (f:unit->'TAbstract, lifetime:Lifetime) = 
        catalog.Add(typeof<'TAbstract>, (Factory(f >> box), lifetime))
#endif 
    member container.Resolve<'TAbstract when 'TAbstract : not struct>() =
        container.Resolve(typeof<'TAbstract>) :?> 'TAbstract     
    member container.Resolve(abstractType:AbstractType) =
        match tryResolve abstractType with
        | Some value -> value
        | None -> sprintf "Failed to resolve %A" abstractType |> invalidOp
    member container.Release(instance:obj) =
        singletons 
        |> Seq.filter (fun pair -> pair.Value = instance)
        |> Seq.toList
        |> List.iter (fun pair -> singletons.Remove(pair.Key) |> ignore)

module Usage =

    let container = Container.Current

    type A () =
        do  printfn "A"

    type B () =
        do  printfn "B"

    type C (a:A, B) =
        do  printfn "C"

    type D (a:A, b:B, fc:unit -> C) =
        let c = fc()
        do printfn "D"

    container.Register((fun () -> printfn "B lambda"; B()), Transient)
    container.Register<C>(typeof<C>)
    let c = container.Resolve<C>()
    let c' = container.Resolve<C>()
    container.Release(c)
    let c'' = container.Resolve<C>()
    container.Release(c'')
    let d = container.Resolve<D>()

    type F() =
        override this.ToString() = "Hey"

    type O (f:Func<F>) =
        let s = f.Invoke()
        do printfn "%A" s 

    container.Register<O>(typeof<O>)
    let o = container.Resolve<O>()

    type ICalculate =
        abstract member Incr : int -> int

    type Calculator () =
        interface ICalculate with
            member this.Incr(x:int) = x + 1
    
    container.Register<ICalculate>(typeof<Calculator>)

    let calc = container.Resolve<ICalculate>()
    printfn "%d" (calc.Incr 1)
    Console.ReadLine() |> ignore