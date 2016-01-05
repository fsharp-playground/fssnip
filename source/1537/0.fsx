// [snippet:Abstract Syntax Tree]
type Name = string
type Event = Event of Name
type State = State of Name
type Action = Action of Name
type Transition = Transition of State * Event * State * Action list
type Logic = Transition list
type Header = Header of Name * Name
// [/snippet]

#if INTERACTIVE
#r @"packages\FParsec.1.0.1\lib\net40-client\FParsecCS.dll"
#r @"packages\FParsec.1.0.1\lib\net40-client\FParsec.dll"
#endif

// [snippet:Parser]
open FParsec

let pname = many1SatisfyL isLetter "name"

let pheader = pname .>> pstring ":" .>> spaces1 .>>. pname .>> spaces
              |>> fun (key,value) -> Header(key,value)

let pstate = pname .>> spaces1 |>> State
let pevent = pname .>> spaces1 |>> Event
let paction = pname |>> Action

let pactions = 
   paction |>> fun action -> [action]
   <|> between (pstring "{") (pstring "}") (many (paction .>> spaces))

let psubtransition =
   pipe3 pevent pstate pactions (fun ev ns act -> ev,ns,act)

let ptransition1 =
   pstate .>>. psubtransition
   |>> fun (os,(ev,ns,act)) -> [Transition(os,ev,ns,act)]

let ptransitionGroup =
   let psub = spaces >>. psubtransition .>> spaces
   pstate .>>. (between (pstring "{") (pstring "}") (many1 psub))
   |>> fun (os,subs) -> [for (ev,ns,act) in subs -> Transition(os,ev,ns,act)]

let ptransitions =
   let ptrans = attempt ptransition1 <|> ptransitionGroup
   between (pstring "{") (pstring "}") (many (spaces >>. ptrans .>> spaces))
   |>> fun trans -> List.collect id trans

let pfsm =
   spaces >>. many pheader .>>. ptransitions .>> spaces

let parse code =
   match run pfsm code with
   | Success(result,_,_) -> result
   | Failure(msg,_,_) -> failwith msg
// [/snippet]

// [snippet:Example 1]
let example1 = "Actions: Turnstile
          FSM: OneCoinTurnstile
          Initial: Locked
          {
            Locked Coin Unlocked {alarmOff unlock}
            Locked Pass Locked  alarmOn
            Unlocked Coin Unlocked thankyou
            Unlocked Pass Locked lock
          }
"
parse example1
(*
val it : Header list * Transition list =
  ([Header ("Actions","Turnstile"); 
    Header ("FSM","OneCoinTurnstile");
    Header ("Initial","Locked")],
   [Transition
      (State "Locked",Event "Coin",State "Unlocked", [Action "alarmOff"; Action "unlock"]);
    Transition 
      (State "Locked",Event "Pass",State "Locked",[Action "alarmOn"]);
    Transition
      (State "Unlocked",Event "Coin",State "Unlocked", [Action "thankyou"]);
    Transition 
      (State "Unlocked",Event "Pass",State "Locked",[Action "lock"])])

*)
// [/snippet]

// [snippet:Example 2]
let example2 = "Actions: Turnstile
          FSM: OneCoinTurnstile
          Initial: Locked{
             Locked {
               Coin Unlocked {alarmOff unlock}
               Pass Locked {alarmOn}
             }
                       
           Unlocked {
               Coin Unlocked {thankyou}
               Pass Locked {lock}
             }
          }
"
parse example2
(*
val it : Header list * Transition list =
  ([Header ("Actions","Turnstile"); 
    Header ("FSM","OneCoinTurnstile");
    Header ("Initial","Locked")],
   [Transition
      (State "Locked",Event "Coin",State "Unlocked", [Action "alarmOff"; Action "unlock"]);
    Transition 
      (State "Locked",Event "Pass",State "Locked",[Action "alarmOn"]);
    Transition
      (State "Unlocked",Event "Coin",State "Unlocked", [Action "thankyou"]);
    Transition 
      (State "Unlocked",Event "Pass",State "Locked",[Action "lock"])])
*)
// [/snippet]