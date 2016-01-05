// [snippet:Abstract Syntax Tree]
type Name = string
type Event = Name
type State = Name
type Action = Name
type Transition = { OldState:State; Event:Event; NewState:State; Actions:Action list }
type Header = Header of Name * Name
// [/snippet]

#if INTERACTIVE
#r @"packages\FParsec.1.0.1\lib\net40-client\FParsecCS.dll"
#r @"packages\FParsec.1.0.1\lib\net40-client\FParsec.dll"
#endif

// [snippet:Parser]
open FParsec

let pname = many1SatisfyL isLetter "name"

let pheader = pname .>> pstring ":" .>> spaces1 .>>. pname .>> spaces |>> Header

let pstate = pname .>> spaces1
let pevent = pname .>> spaces1
let paction = pname

let pactions = 
   paction |>> fun action -> [action]
   <|> between (pstring "{") (pstring "}") (many (paction .>> spaces))

let psubtransition =
   pipe3 pevent pstate pactions (fun ev ns act -> ev,ns,act)

let ptransition1 =
   pstate .>>. psubtransition
   |>> fun (os,(ev,ns,act)) -> [{OldState=os;Event=ev;NewState=ns;Actions=act}]

let ptransitionGroup =
   let psub = spaces >>. psubtransition .>> spaces
   pstate .>>. (between (pstring "{") (pstring "}") (many1 psub))
   |>> fun (os,subs) -> 
      [for (ev,ns,act) in subs -> {OldState=os;Event=ev;NewState=ns;Actions=act}]

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

// [snippet: Compiler]
let compile (headers,transitions) =
   let header name = 
      headers |> List.pick (function Header(key,value) when key = name -> Some value | _ -> None)
   let states = 
      transitions |> List.collect (fun trans -> [trans.OldState;trans.NewState]) |> Seq.distinct      
   let events =
      transitions |> List.map (fun trans -> trans.Event) |> Seq.distinct  

   let initial = header "Initial"
   "package thePackage;\n" +
   (sprintf "public abstract class %s implements %s {\n" (header "FSM") (header "Actions")) +
   "\tpublic abstract void unhandledTransition(String state, String event);\n" +
   (sprintf "\tprivate enum State {%s}\n" (String.concat "," states)) +
   (sprintf "\tprivate enum Event {%s}\n" (String.concat "," events)) +
   (sprintf "\tprivate State state = State.%s;\n" (header "Initial")) +
   "\tprivate void handleEvent(Event event) {\n" +
   "\t\tswitch(state) {\n" +
   (String.concat ""
      [for (oldState,ts) in transitions |> Seq.groupBy (fun t -> t.OldState) ->
         (sprintf "\t\t\tcase %s:\n" oldState) +
         "\t\t\t\tswitch(event) {\n" +
         (String.concat ""
            [for t in ts ->
               (sprintf "\t\t\t\t\tcase %s:\n" t.Event) +
               (sprintf "\t\t\t\t\t\tsetState(State.%s);\n" t.NewState)+
               (String.concat ""
                  [for a in t.Actions -> sprintf "\t\t\t\t\t\t%s();\n" a]
               ) +
               "\t\t\t\t\tbreak;\n"
            ]         
         ) +
         "\t\t\t\t\tdefault: unhandledTransition(state.name(), event.name()); break;\n" +
         "\t\t\t\t}\n" +
         "\t\t\t\tbreak;\n"
      ] 
   )+   
   "\t\t}\n" +
   "\t}\n" +   
   "}\n"
// [/snippet]

// [snippet:Example]
let example = "Actions: Turnstile
          FSM: OneCoinTurnstile
          Initial: Locked
          {
            Locked Coin Unlocked {alarmOff unlock}
            Locked Pass Locked  alarmOn
            Unlocked Coin Unlocked thankyou
            Unlocked Pass Locked lock
          }
"
example |> parse |> compile
(*
val it : string =
  "package thePackage;
public abstract class OneCoinTurnstile implements Turnstile {
	public abstract void unhandledTransition(String state, String event);
	private enum State {Locked,Unlocked}
	private enum Event {Coin,Pass}
	private State state = State.Locked;
	private void handleEvent(Event event) {
		switch(state) {
			case Locked:
				switch(event) {
					case Coin:
						setState(State.Unlocked);
						alarmOff();
						unlock();
					break;
					case Pass:
						setState(State.Locked);
						alarmOn();
					break;
					default: unhandledTransition(state.name(), event.name()); break;
				}
				break;
			case Unlocked:
				switch(event) {
					case Coin:
						setState(State.Unlocked);
						thankyou();
					break;
					case Pass:
						setState(State.Locked);
						lock();
					break;
					default: unhandledTransition(state.name(), event.name()); break;
				}
				break;
		}
	}
}
"
*)
// [/snippet]
