open System
open System.Collections.Generic

// event handler
type Handler = (unit->unit)

// imitates dialog system
let Dialogs = new Dictionary<string*string, bool>()
let DialogHandlers = new Dictionary<string*string, Handler>()
// imitates killing events
let Kills = new Dictionary<string, int>()
let KillHandlers = new Dictionary<string, Handler>()

type Player() =
    member x.GiveXP n = 
        printfn "Gained %d experience points" n
    member x.StartQuest(quest) =
        printfn "Starting quest '%s'" quest
    member x.GiveReward(reward) =
        printfn "Rewarding player with '%s'" reward

    // simulating players actions
    member x.TalkTo(npc, node) =
        if Dialogs.[(npc, node)] then
            try DialogHandlers.[npc, node]() with | :? KeyNotFoundException -> printfn "No event is attached to the node"
        else
            printfn "Dialog node '%s' is not attached to npc '%s'" node npc
    member x.Kill(npc) =
        if not (Kills.ContainsKey(npc)) then Kills.[npc] <- 0
        Kills.[npc] <- Kills.[npc] + 1
        KillHandlers.[npc]()
    // simulating conditions
    member x.KillCount(npc) =
        try Kills.[npc] with | :? KeyNotFoundException -> 0

type Dialog(node, npc, at) = // for now we scrap 'at'. we don't care about structure yet
    do 
        if not (Dialogs.ContainsKey(npc, at)) then 
            printfn "Attaching dialog '%s' for npc '%s' at node '%s'" node npc at
            Dialogs.[(npc, node)] <- true

    member x.Disable() =
        printfn "Disabling dialog node '%s' for npc '%s'" node npc
        Dialogs.[(npc, node)] <- false

type Event =
    | Talk of Player * string * string // player talks with npc using given node
    | Kill of Player * string // player kills npc
type Quest = 
    | Phase of Event * Handler
    | Nothing

type QuestBuilder() =
    member x.Bind(v : Event, f) = 
        match v with
        | Talk(player, npc, node) -> 
            printfn "Player needs to talk with %s using node %s" npc node
            DialogHandlers.[(npc, node)] <- f // assign handler to dialog
            Talk(player, npc, node), f // or we could use that return type to do that?
        | Kill(player, npc) -> 
            if player.KillCount(npc) > 0 then
                printfn "Player already killed %s" npc
                // fire event immediately then
                f()
            else
                printfn "Player needs to kill %s" npc
                KillHandlers.[npc] <- f
            Kill(player, npc), f
        |> ignore

    member x.Bind(v : Event * Event, f) =
        printfn "bind event*event"
        match v with
            | e1, e2 ->
                x.Bind(e1, f)
                printfn "or"
                x.Bind(e2, f)
    member x.Bind(v : unit, f) =
        printfn "unit bind"
        f
    member x.Zero() = 
        printfn "Zero"
        ()
    member x.Return(v) =
        printfn "Return"
        v

let talk player npc node = Talk(player, npc, node)
let kill player npc = Kill(player, npc)
    
let quest = new QuestBuilder()

let beast = quest {
    let player = new Player()
    let intro = new Dialog("Beast", "Rudolf", "Hello") // let's attach dialog named "Beast", to the given npc at given node

    // this is the beginning, we require player to talk with an npc using specified dialog node
    // but should he kill the beast before, quest is gonna start as well
    do! talk player "Rudolf" "Beast", kill player "Beast"
    player.StartQuest("Beast") // and then, the rest will be called when player does this
    intro.Disable(); // we no longer want it to be accessible, instead of we will attach:
    let about = new Dialog("AboutThatBeast", "Rudolf", "Hello")

    do! kill player "Beast", talk player "Beast" "Diplomacy"
    if player.KillCount("Beast") = 1 then
        player.GiveXP(200)
        about.Disable()
        let reward = new Dialog("BeastReward", "Rudolf", "Hello")
        do! talk player "Rudolf" "BeastReward"
        reward.Disable();
        player.GiveReward("Awesome sword of Moonshinin'");
} 

// let's simulate player
let player = new Player()
//player.TalkTo("Rudolf", "KillBeast")
//player.Kill("Beast")

