open System

type Location = Room | Garden | Attic
type Direction = North | South | East | West | Up | Down

type Thing = { Name: string; Article: string }
type Edge = { Dir: Direction; Portal: string; Loc: Location }

type Player(initial: Location, objects: ResizeArray<Thing>) =
    let mutable location = initial
    member this.PickUp(obj)  =
        objects.Add(obj)
    member this.Location 
        with get() = location
        and  set v = location <- v

type World() =
    let player = new Player(Room, new ResizeArray<Thing>())
    let mutable objects =
        [Room, [{ Name = "whiskey"; Article = "some" }; { Name = "bucket"; Article = "a" }];
        Garden, [{ Name = "chain"; Article = "a length of" }];
        Attic, []]
        |> Map.ofList
    let locations =
        [Room, "You are in a living room. A wizard is snoring loudly on the couch.";
        Garden, "You are in a beautiful garden. There is a well here.";
        Attic, "You are in an attic. There is a giant welding torch in the corner."]
        |> Map.ofList
    let edges =
        [Room, [{ Dir = West; Portal = "door"; Loc = Garden }; { Dir = Up; Portal = "ladder"; Loc = Attic }];
        Garden, [{ Dir = East; Portal = "door"; Loc = Room }];
        Attic, [{ Dir = Down; Portal = "ladder"; Loc = Room }]]
        |> Map.ofList
    member this.Locations = locations
    member this.Edges = edges
    member this.Player = player
    member this.Objects
        with get() = objects
        and  set v = objects <- v


let asOne (strs : seq<_>) =
    String.Join(" ", strs)
 
let describePath edge =
    (sprintf "%A" edge.Dir).ToLower() |> sprintf "There is a %s going %s from here." edge.Portal
 
let describePaths edges =
    edges |> Seq.map describePath |> asOne


let describeObjects objs =
    let describeObj obj = sprintf "You see here %s %s." obj.Article obj.Name
    objs |> Seq.map describeObj |> asOne

let look (world: World) =
    let player = world.Player
    let loc = world.Locations.[player.Location]
    let paths = world.Edges.[player.Location] |> describePaths
    let objs = world.Objects.[player.Location] |> describeObjects
    [loc; paths; objs] |> asOne

let walk dir (world: World) =
    let player = world.Player
    let attempt = world.Edges.[player.Location]
                  |> List.filter (fun e -> e.Dir = dir)
    match attempt with
    | [] -> "You can't go that way."
    | edge :: _ -> 
        world.Player.Location <- edge.Loc
        sprintf "You go %A..." dir

let pickUp thing (world: World) =
    let player = world.Player
    let objs = world.Objects.[player.Location]
    let attempt = objs |> List.partition (fun o -> o.Name = thing)
    match attempt with
    | [], _ -> "You cannot get that."
    | thing :: [], things ->
        world.Player.PickUp thing
        world.Objects <- world.Objects.Remove(player.Location)
        world.Objects <- world.Objects.Add(player.Location, things)
        sprintf "You are now carrying %s %s." thing.Article thing.Name
    | _ -> "I don't know what you mean."

let getLastWord (phrase: string) =
    let words = phrase.Split(' ')
    words.[words.Length - 1]
 
let parsePickUp cmd world =
    let obj = getLastWord cmd
    pickUp obj world |> printfn "%s"
 
let parseWalk (dir : string) world =
    let direction = 
        match dir with
        | d when d.StartsWith("e") -> Some(East)
        | d when d.StartsWith("n") -> Some(North)
        | d when d.StartsWith("s") -> Some(South)
        | d when d.StartsWith("w") -> Some(West)
        | d when d.StartsWith("u") -> Some(Up)
        | d when d.StartsWith("d") -> Some(Down)
        | _ -> None
    match direction with
    | Some(d) -> walk d world |> printfn "%s"
    | None -> printfn "You can't go that way."
 
let parseMovement cmd world =
    let dir = getLastWord cmd
    parseWalk dir world
 
let rec handleInput world =
    printf "> "
    let input = stdin.ReadLine().Trim().ToLower()
    match input with
    | "quit" | "exit" | "leave" -> 
        printfn "Goodbye!" 
        world, true
    | "look" -> world, false
    | "n" | "s" | "e" | "w" | "u" | "d" | "up" | "down" | "north" | "south" | "east" | "west" ->
        parseWalk input world
        world, false
    | cmd when cmd.StartsWith("go ") || cmd.StartsWith("walk") || cmd.StartsWith("climb")  ->
        parseMovement cmd world
        world, false
    | cmd when cmd.StartsWith("take") || cmd.StartsWith("get") || cmd.StartsWith("pick")  ->
        parsePickUp cmd world
        world, false
    | other ->
        printfn "You can't %s here." other
        handleInput world
 
let rec gameLoop world =
    look world |> printfn "%s"
    match handleInput world with
    | _, true -> ()
    | w, _ -> gameLoop w
 
World() |> gameLoop