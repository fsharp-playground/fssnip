// Put all the strategies as a static members for a class that can't be instantiated

type GoStrategies =
    static member Driving = printfn "I'm Driving"
    static member Flying = printfn "I'm Flying"
    static member Swimming = printfn "I'm Swimming"

[<AbstractClassAttribute>]
type Vehicle() =
    member val Brand = "" with get,set
    member val Model = "" with get,set
    abstract Type :string with get
    abstract member Go :unit -> unit
    member O.Print() = printfn "%s\t%s\t%s" O.Type O.Brand O.Model

type Car() =
    inherit Vehicle()
    override O.Type with get() = "Car"
    override O.Go() = GoStrategies.Driving

type Plane() =
    inherit Vehicle()
    override O.Type with get() = "Plane"
    override O.Go() = GoStrategies.Flying

type Boat() =
    inherit Vehicle()
    override O.Type with get() = "Boat"
    override O.Go() = GoStrategies.Swimming

// Testing
let car1 = Car( Brand="BMW", Model="X6" )
let car2 = Car( Brand="Dodge", Model="Viper" )
let plane1 = Plane( Brand="AirBus", Model="A321" )
car1.Print()
car1.Go()
car2.Print()
car2.Go()
plane1.Print()
plane1.Go()