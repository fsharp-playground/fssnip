(* Having witnessed a painful attempt at the Strategy design pattern I was inspired
to share some of the little knowledge I have.

If you are unsure whether you got the pattern right you should always ask yourself
the question: What problem is it trying to solve?

If you are not solving the following problem, then it isn't called the Strategy pattern:

    How can I modify the behavior of a class (even at run-time)
    without having to change its implementation.

The general idea is to parameterize the class with the implementation of another interface
passed in via the constructor.

Our class then delegates the behavior to the implementation it's being passed: *)

type IMakeNoise =
    abstract Talk: Animal -> unit

and Animal(name: string, makeNoise: IMakeNoise) =
    member x.Name = name
    member this.Talk() = makeNoise.Talk this

type FrogNoise() =
    interface IMakeNoise with
        member x.Talk frog = printfn "%s croaks" frog.Name

type MouseNoise() =
    interface IMakeNoise with
        member x.Talk _ = printfn "A mouse squeals"

let frog = Animal("Billy", FrogNoise())
let mouse = Animal("", MouseNoise())

frog.Talk()
mouse.Talk()

(* Notice a few things:

-   We have a single implementation of animals and adapting the behavior of the animal
    talking will not require a change to its implementation.
-   We have the choice of defining the interface IMakeNoise with or without accepting
    the Animal instance it will act for:
    -   Passing the instance to the talk function makes it more flexible,
        notice how I couldn't have implemented the "nameless" mouse squeal
        without changing our Animal implementation otherwise.
    - On the other hand, this requires a cycle, for which I invite you
      to read Scott Wlaschin's excellent serie on the subject:
      http://fsharpforfunandprofit.com/series/dependency-cycles.html

Making the interface generic breaks the cycle, is more flexible and preserves type safety: *)

type IMakeNoise<'T> =
    abstract Talk: 'T -> unit

type Creature(name: string, makeNoise: IMakeNoise<Creature>) =
    member __.Name = name
    member this.Talk() = makeNoise.Talk this

(* A great feature of the F# type system is Object Expression which allow you to instantiate
an object/interface without the need for an explicit class definition: *)

let horse =
    let makeNoise =
        {
            new IMakeNoise<Creature> with
                member __.Talk horse = printfn "%s snorts" horse.Name
        }
    Creature("Fred", makeNoise)

horse.Talk()

(* We said that our class *delegates* the responsibility to an implementation of IMakeNoise.
This is where the difference between the Delegation and Strategy pattern lies,
wherea the Delegation pattern has our class instantiate the class to which it will delegate
the work, the Strategy pattern has the implementation passed in at construction.

Being more flexible it can also be done at run-time making our code
less vulnerable to evolving requirements.

Another concept we know from C# called a delegate which is a poor man's function type. Our
IMakeNoise interface defines a single method with a fixed signature and can thus be expressed
without having to rely on the interface at all: Creature -> Unit *)

type Monster(name: string, makeNoise: Monster -> unit) =
    member __.Name = name
    member x.Talk() = makeNoise x

let troll = Monster("Grendel", fun troll -> printfn "%s bellows" troll.Name)

troll.Talk()

(* Object Expression can push the limit further switching the class to an interface.

We can also untangle the constructor from the type and still avoid any code duplication.
Notice how you could squeeze a Factory pattern for free right there. *)

type IMonster =
    abstract Name: string
    abstract Talk: unit -> unit

let monster name makeNoise =
    {
        new IMonster with
            member __.Name = name
            member x.Talk() = makeNoise x
    }

let dragon = monster "Onyxia" (fun d -> printfn "%s roars" d.Name)
let whelp = monster "" (fun _ -> printfn "The whelp screeches")

dragon.Talk()
whelp.Talk()

(* You can see how this is at least as flexible and we have maintained type safety.

Records also allows us to untangle the type and a fixed class *)

type Beast =
    {
        Name: string
        Talk: unit -> unit
    }

let beast name makeNoise =
    {
        Name = name
        Talk = fun () -> makeNoise name
    }

let thunders = printfn "%s thunders"

let hulk = beast "The Hulk" thunders

hulk.Talk()

(* At this point you really have to squint to see an OO design pattern as this is much
closer to functional programming.

In fact functional programmers tend not to label this at all since programming with
higher order function is simply called "Programming".

Some might argue this is still object oriented as an F# Record is implemented as a
class under the cover. There is also the fact that our beast instance closes over our
thunders function.

One should notice that inheritance isn't possible when using the Record, on the other
hand many good OO programmers would argue that inheritance is misused more often than
not. And being a type as opposed to a class, the record allows for multiple implementations.

Mutation whose bad usage tend to outnumber the good ones 10 to 1 isn't OO per-se but
is frequently seen as OO's natural companion. Without making it impossible, Records
thwart you from using mutation which will generally lead you to better designs.

A commonly seen case for mutation gone awry that also shows poor understanding of Design
patterns is when you see a factory that generates instances of a class who's been endowed
with a parameterless constructor and uses mutable properties to set the instance's state.
This is an obvious misuse of the pattern as one of the roles of the constructor is to prevent
the creation of instances in an invalid state. *)