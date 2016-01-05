open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

type Message = string
exception TypeResolutionException of Message * Type
type Lifetime = Singleton | Transient
type AbstractType = Type
type ConcreteType = Type
type private Constructor = Reflected of ConcreteType | Factory of (unit -> obj)
let private (|FunType|_|) t =
    if FSharpType.IsFunction t then FSharpType.GetFunctionElements t |> Some
    else None
let private (|SeqType|_|) (t:Type) =
    if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>
    then t.GetGenericArguments().[0] |> Some
    else None
let private toOption = function Choice1Of2 x -> Some x | Choice2Of2 _ -> None
/// IoC Container
type Container () as container =
    let catalog = Dictionary<AbstractType, Constructor * Lifetime>()
    let singletons = Dictionary<ConcreteType,obj>()
    let rec tryResolve cs t =
        match catalog.TryGetValue t with
        | true, (Reflected u , lifetime) -> 
            tryObtain u (fun () -> tryReflect cs u) lifetime
        | true, (Factory f, lifetime) -> 
            tryObtain t (fun () -> f() |> Choice1Of2) lifetime
        | false, _ -> 
            tryObtain t (fun () -> tryReflect cs t) Singleton 
    and tryObtain t f lifetime =
        match singletons.TryGetValue t with
        | true, value -> Choice1Of2(value)
        | false, _ ->
            let result = f()
            result |> function Choice1Of2 value -> store t value lifetime | Choice2Of2 _ -> ()
            result
    and store t value = function Singleton -> singletons.Add(t,value) | Transient -> ()
    and tryReflect cs t =
        if cs |> List.exists ((=) t) 
        then Choice2Of2 "Cycle detected"
        else tryConstructors (t::cs) t
    and tryConstructors cs t =
        t.GetConstructors()
        |> Array.sortBy (fun c -> c.GetParameters().Length)
        |> Array.tryPick (tryConstructor cs >> toOption)
        |> function Some value -> Choice1Of2 value | None -> Choice2Of2 "Failed to find matching constructor"
    and tryConstructor cs ci =
        let ps = ci.GetParameters()
        let args = ps |> Array.choose (fun p -> tryResolveArgument cs p.ParameterType |> toOption)
        if args.Length = ps.Length then args |> ci.Invoke |> Choice1Of2
        else Choice2Of2 "Failed to resolve all parameters of constructor"
    and tryResolveArgument cs t =
        match t with
        | FunType(arg,result) when arg = typeof<unit> ->
            FSharpValue.MakeFunction(t,fun args -> container.Resolve(result)) |> Choice1Of2
        | SeqType t ->
            let interfaces = catalog.Keys |> Seq.filter (fun x -> x.GetInterfaces() |> Seq.exists ((=) t))
            let subTypes = catalog.Keys |> Seq.filter (fun x -> x.IsSubclassOf t)
            let values =
                Seq.append interfaces subTypes 
                |> Seq.choose (tryResolve cs >> toOption) 
                |> Seq.toArray
            let result = Array.CreateInstance(t, values.Length)
            Array.Copy(values, result, values.Length)
            result |> box |> Choice1Of2
        | t -> tryResolve cs t
    /// Register sequence of abstract types against specified concrete type
    member container.Register(abstractTypes:AbstractType seq, concreteType:ConcreteType) =
        for t in abstractTypes do catalog.Add(t, (Reflected concreteType, Singleton))
    /// Register abstract type against specified type instance
    member container.Register<'TAbstract>(instance:'TAbstract) =
        catalog.Add(typeof<'TAbstract>, (Reflected typeof<'TAbstract>, Singleton))
        singletons.Add(typeof<'TAbstract>, instance)
    /// Register abstract type against specified concrete type with given lifetime
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (concreteType:ConcreteType, lifetime:Lifetime) =
        let abstractType = typeof<'TAbstract>
        if concreteType <> abstractType &&
           not (concreteType.IsSubclassOf(abstractType)) &&
           not (concreteType.GetInterfaces() |> Array.exists ((=) abstractType)) then
            invalidArg "concreteType" "Concrete type does not implement abstract type"
        catalog.Add(abstractType, (Reflected concreteType, lifetime))
    /// Register abstract type against specified factory with given lifetime
    member container.Register<'TAbstract when 'TAbstract : not struct>
            (f:unit->'TAbstract, lifetime:Lifetime) = 
        catalog.Add(typeof<'TAbstract>, (Factory(f >> box), lifetime))
    /// Resolve instance of specified abstract type
    member container.Resolve<'TAbstract when 'TAbstract : not struct>() =
        container.Resolve(typeof<'TAbstract>) :?> 'TAbstract
    /// Resolve instsance of specified abstract type
    member container.Resolve(abstractType:AbstractType) =
        match tryResolve [] abstractType with
        | Choice1Of2 value -> value
        | Choice2Of2 message -> TypeResolutionException(message,abstractType) |> raise
    /// Remove instance reference from container
    member container.Release(instance:obj) =
        singletons |> Seq.filter (fun pair -> pair.Value = instance) |> Seq.toList
        |> List.iter (fun pair -> singletons.Remove(pair.Key) |> ignore)

open NUnit.Framework

[<TestFixture>]
module ``Container Register, Resolve, Release Tests`` =
    
    [<AbstractClass>]
    type AbstractType () = do ()

    type ConcreteType () = inherit AbstractType()

    type IMarkerInterface = interface end

    type MarkedType () = interface IMarkerInterface
    
    let [<Test>] ``registering 2 instances of an abstract type in a single container should throw`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<AbstractType>, Singleton)
        Assert.Throws<System.ArgumentException>(fun () ->
            container.Register<AbstractType>(typeof<AbstractType>, Singleton) |> ignore
        ) |> ignore

    let [<Test>] ``attempting to resolve an unregistered type should throw`` () =
        let container = Container()
        Assert.Throws<TypeResolutionException>(fun () ->  
            container.Resolve<AbstractType>() |> ignore
        ) |> ignore

    let [<Test>] ``resolving a registered abstract type should return an instance of the specified concrete type`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConcreteType>, Singleton)
        let instance = container.Resolve<AbstractType>()
        Assert.True(instance :? ConcreteType)

    let [<Test>] ``resolving a type with a singleton lifetime should always return the same instance`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConcreteType>, Singleton)
        let a = container.Resolve<AbstractType>()
        let b = container.Resolve<AbstractType>()
        Assert.True( Object.ReferenceEquals(a,b) )
        
    let [<Test>] ``resolving a type with a transient lifetime should a new instance each time`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConcreteType>, Transient)
        let a = container.Resolve<AbstractType>()
        let b = container.Resolve<AbstractType>()
        Assert.AreNotSame(a,b)

    let [<Test>] ``resolving a registered instance of a type should return that instance`` () =
        let container = Container()
        let this = ConcreteType()
        container.Register<AbstractType>(this)
        let that = container.Resolve<AbstractType>()
        Assert.AreSame(this, that)

    let [<Test>] ``resolving a type registered as a factory should call the specified factory`` () =
        let called = ref false
        let factory = fun () -> called := true; ConcreteType() :> AbstractType
        let container = Container()
        container.Register<AbstractType>(factory, Singleton)
        container.Resolve<AbstractType>() |> ignore
        Assert.True( called.Value )

    let [<Test>] ``releasing a registered concrete instance then resolving the type should return a new concrete instance`` () =
        let container = Container()
        let this = ConcreteType()
        container.Register<ConcreteType>(this)
        container.Release(this)
        let that = container.Resolve<ConcreteType>()
        Assert.True( not <| Object.ReferenceEquals(this, that) )

    do
        ``registering 2 instances of an abstract type in a single container should throw`` ()
        ``attempting to resolve an unregistered type should throw`` ()
        ``resolving a registered abstract type should return an instance of the specified concrete type``  ()
        ``resolving a type with a singleton lifetime should always return the same instance`` ()
        ``resolving a type with a transient lifetime should a new instance each time`` ()
        ``resolving a registered instance of a type should return that instance`` ()
        ``resolving a type registered as a factory should call the specified factory`` ()
        ``releasing a registered concrete instance then resolving the type should return a new concrete instance`` ()

[<TestFixture>]
module ``Constructor Tests`` =
    
    [<AbstractClass>]
    type AbstractType () = do ()
   
    type ConstructorWithValueTypeArg (arg:int) = inherit AbstractType()

    let [<Test>] ``resolving type with value type dependency in constructor should throw`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConstructorWithValueTypeArg>, Singleton)
        Assert.Throws<TypeResolutionException>(fun () ->
            container.Resolve<AbstractType>() |> ignore
        ) |> ignore

    type ReferenceType() = do ()
    type ConstructorWithReferenceTypeArg (arg:ReferenceType) = inherit AbstractType()

    let [<Test>] ``resolving type with reference type dependency in constructor should inject reference`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConstructorWithReferenceTypeArg>, Singleton)
        let instance = container.Resolve<AbstractType>()
        Assert.NotNull(instance)

    type ConstructorWithSelfReferenceArg (arg:AbstractType) = inherit AbstractType()

    let [<Test>] ``resolving type with self type dependency in constructor should fail`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConstructorWithSelfReferenceArg>, Singleton)
        Assert.Throws<TypeResolutionException>(fun () ->
                container.Resolve<AbstractType>() |> ignore
        ) |> ignore

    type ConstructorWithFunArg (arg:unit -> ReferenceType) = 
        inherit AbstractType()
        member this.Factory () = arg()

    let [<Test>] ``resolving type with fun type argument in constructor should inject factory`` () =
        let container = Container()
        container.Register<AbstractType>(typeof<ConstructorWithFunArg>, Singleton)
        let instance = container.Resolve<AbstractType>() :?> ConstructorWithFunArg
        let refValue = instance.Factory()
        Assert.NotNull(refValue)

    type SubType1 () = inherit AbstractType()
    type SubType2 () = inherit AbstractType()
    type ConstructorWithSeqArg (subTypes:AbstractType seq) =
        member this.SubTypes = subTypes

    let [<Test>] ``resolving type with seq type argument in constructor should inject sub types`` () =
        let container = Container()
        container.Register<SubType1>(typeof<SubType1>, Singleton)
        container.Register<SubType2>(typeof<SubType2>, Singleton)
        container.Register<ConstructorWithSeqArg>(typeof<ConstructorWithSeqArg>, Singleton)
        let instance = container.Resolve<ConstructorWithSeqArg>()
        let types = instance.SubTypes |> Seq.map (fun i -> i.GetType().Name) |> Set.ofSeq
        let types' = set [ typeof<SubType1>.Name; typeof<SubType2>.Name ]
        Assert.That((types = types'))

    type Marker = interface end
    type MarkedType1 () = interface Marker
    type MarkedType2 () = interface Marker
    type ConstructorWithInterfaceArg (markedTypes:Marker seq) =
        member this.MarkedTypes = markedTypes

    let [<Test>] ``resolving type with seq type argument in constructor should inject interfaces`` () =
        let container = Container()
        container.Register<MarkedType1>(typeof<MarkedType1>, Singleton)
        container.Register<MarkedType2>(typeof<MarkedType2>, Singleton)
        container.Register<ConstructorWithInterfaceArg>(typeof<ConstructorWithInterfaceArg>, Singleton)
        let instance = container.Resolve<ConstructorWithInterfaceArg>()
        let types = instance.MarkedTypes |> Seq.map (fun i -> i.GetType().Name) |> Set.ofSeq
        let types' = set [ typeof<MarkedType1>.Name; typeof<MarkedType2>.Name ]
        Assert.That((types = types'))

    do  ``resolving type with value type dependency in constructor should throw`` ()
        ``resolving type with reference type dependency in constructor should inject reference`` ()
        ``resolving type with self type dependency in constructor should fail`` ()
        ``resolving type with fun type argument in constructor should inject factory`` ()
        ``resolving type with seq type argument in constructor should inject sub types`` ()
        ``resolving type with seq type argument in constructor should inject interfaces`` ()

module Usage =

    type ICalculate =
        abstract member Incr : int -> int

    type Calculator () =
        interface ICalculate with
            member this.Incr(x:int) = x + 1
    
    let container = Container()

    // Register
    container.Register<ICalculate>(typeof<Calculator>, Singleton)

    // Resolve
    let calc = container.Resolve<ICalculate>()
    
    printfn "%d" (calc.Incr 1)
    
    // Release
    container.Release(calc)

    Console.ReadLine() |> ignore