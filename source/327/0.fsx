// Miss Grant's Controller External DSL with F# parser
// See Domain-Specific Languages: An Introductory Example by Martin Fowler
// http://www.informit.com/articles/article.aspx?p=1592379&seqNum=3

/// Name type abbreviation
type name = string
/// Code type abbreviation
type code = string

/// State Machine record type
type Machine = { 
    events : (name * code) list
    resetEvents: name list
    commands : (name * code) list
    states : (name * State) list
    } with 
    static member empty =
        { events = []; resetEvents = []; commands = []; states = [] }
and State = { actions: name list; transitions: (name * name) list }
    with static member empty = { actions=[]; transitions=[] }
     
let whitespace = " \t\r\n".ToCharArray()
let parseError s = invalidOp s

/// Returns new machine with values parsed from specified text
let rec parse (machine:Machine) = function
    | "events"::xs -> events machine xs
    | "resetEvents"::xs -> resetEvents machine xs
    | "commands"::xs -> commands machine xs
    | "state"::name::xs -> 
        let state',xs = parseState (State.empty) xs
        let machine' = { machine with states = (name,state')::machine.states }
        parse machine' xs
    | [] -> machine
    | x::_ -> "unknown token " + x |> parseError
/// Parses event declarations until end token is reached
and events machine = function
    | "end"::xs -> parse machine xs
    | name::code::xs -> 
        let event = (name,code)
        let machine' = { machine with events = event::machine.events }
        events machine' xs
    | _ -> parseError "events"
/// Parses reset event declarations until end token is reached
and resetEvents machine = function
    | "end"::xs -> parse machine xs
    | name::xs -> 
        let machine' = { machine with resetEvents = name::machine.resetEvents }
        resetEvents machine' xs
    | _ -> parseError "resetEvents"
/// Parses command declarations until end token is reached
and commands machine = function
    | "end"::xs -> parse machine xs
    | name::code::xs ->
        let command = (name,code)
        let machine' = { machine with commands = command::machine.commands }
        commands machine' xs
    | _ -> parseError "commands"
/// Parses state declaration until end token is reached
and parseState state = function
    | "end"::xs -> state,xs
    | "actions"::xs ->
        let actions', xs = actions xs  
        let state' = { state with actions = actions'@state.actions }
        parseState state' xs
    | event::"=>"::action::xs ->        
        let transition = (event,action)
        let state' = { state with transitions = transition::state.transitions }
        parseState state xs 
    | _ -> parseError "state"
/// Parses action names in curly braces
and actions (xs:string list) = 
    /// Returns text inside curly braces scope
    let rec scope acc = function
        | (x:string)::xs when x.Contains("}") -> 
            (String.concat "" acc).Trim([|'{';'}'|]), xs
        | x::xs -> scope (x::acc) xs
        | [] -> invalidOp "scope"
    let s, xs = scope [] xs
    s.Split(whitespace) |> Array.toList, xs

/// DSL specification
let text = "
events
 doorClosed D1CL
 drawerOpened D2OP
 lightOn L1ON
 doorOpened D1OP
 panelClosed PNCL end
 
resetEvents
 doorOpened 
end

commands
 unlockPanel PNUL
 lockPanel PNLK
 lockDoor D1LK
 unlockDoor D1UL
end 	

state idle	
 actions {unlockDoor lockPanel}
 doorClosed => active 
end 

state active
 drawerOpened => waitingForLight
 lightOn => waitingForDrawer 
end 

state waitingForLight
 lightOn => unlockedPanel 
end 

state waitingForDrawer
 drawerOpened => unlockedPanel 
end 

state unlockedPanel
 actions {unlockPanel lockDoor}
 panelClosed => idle 
end"

/// Machine built from DSL text
let machine =
    text.Split(whitespace, System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.toList
    |> parse Machine.empty