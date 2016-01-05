// Miss Grant's Controller as an Internal DSL in F#
// See Domain-Specific Languages: An Introductory Example by Martin Fowler
// http://www.informit.com/articles/article.aspx?p=1592379&seqNum=3

// [omit:Semantic model type definitions]
type code = string
type event = Event of code
type command = Command of code
type transition = Transition of event * (unit -> state)
and  state = State of command seq * transition seq
// [/omit]

// [omit:Internal DSL helper functions]
let event = Event
let command = Command
let state actions transitions = State(actions,transitions)
let (=>) event state = Transition(event,state)
// [/omit]

let doorClosed =    event "D1CL"
let drawerOpened =  event "D2OP"
let lightOn =       event "L1ON"
let doorOpened =    event "D1OP"
let panelClosed =   event "PNCL"

let unlockPanel =   command "PNUL"
let lockPanel =     command "PNLK"
let lockDoor =      command "D1LK"
let unlockDoor =    command "D1UL"

let rec idle () = 
    state
        [unlockDoor; lockPanel]
        [doorClosed => active]
and active () =
    state [] [lightOn => waitingForDrawer]
and waitingForLight () =
    state [] [lightOn => unlockedPanel]
and waitingForDrawer () =
    state [] [drawerOpened => unlockedPanel]
and unlockedPanel () =
    state
        [unlockPanel; lockDoor]
        [panelClosed => idle]