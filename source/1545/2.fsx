// [snippet:Abstract Syntax Tree]
type label = string
type name = string
type arithmetic = Add | Subtract | Multiply | Divide | Power
type value = 
   | String of string 
   | Integer of int
   | Pattern of pattern
and expression = 
   | Value of value
   | Variable of name
   | Arithmetic of expression * arithmetic * expression
and pattern =
   | Expression of expression
   | Or of pattern list
   | ConditionalAssignment of pattern * name
   | Len of int
   | Rem
type transferOn = Success | Failure | Any
type transfer = { On:transferOn; Goto:label }
type command =
   | Assign of name * expression list
   | Match of expression list * pattern list
   | Unit
type line = {
    Label : label option
    Command : command
    Transfer : transfer option
    }
// [/snippet]

// [snippet:Interpereter]
let toString = function 
   | String s -> s 
   | Integer n -> n.ToString()
   | Pattern _ -> invalidOp ""
let (|AsInteger|_|) s =
   match System.Int32.TryParse(s) with
   | true, n -> Some n
   | false, _ -> None
type environment = {     
   Subject:string;
   Cursor:int;
   Result:value;
   Success:bool }
let run input output (lines:line list) =
   let variables = System.Collections.Generic.Dictionary<name, value>()
   let assign name value =
      match name, value with
      | "OUTPUT", Pattern(_) -> name |> output
      | "OUTPUT", _ -> value |> toString |> output
      | _, _ -> variables.[name] <- value
   let get name =
      match name with
      | "INPUT" -> input () |> String
      | _ -> variables.[name]
   let rec concat expressions =      
      System.String.Concat [for e in expressions -> evaluate e |> toString] |> String
   and evaluate expression =      
      match expression with
      | Value(value) -> value
      | Variable(name) -> get name
      | Arithmetic(lhs,op,rhs) ->
         match evaluate lhs, evaluate rhs with
         | Integer l, Integer r -> Integer(arithmetic op l r)
         | Integer l, String (AsInteger r) -> Integer(arithmetic op l r)
         | _ -> invalidOp "Illegal data type"
   and arithmetic op l r =
      match op with
      | Add -> l + r
      | Subtract -> l - r
      | Multiply -> l * r
      | Divide -> l / r
      | Power -> pown l r
   and tryPattern (env:environment) pattern =
      match pattern with
      | Expression(expression) ->
         let subject = env.Subject.Substring(env.Cursor)
         let value = evaluate expression
         match value with
         | Pattern pattern -> tryPattern env pattern
         | value ->  
            let value = value|> toString
            if env.Cursor = 0 then
               let index = subject.IndexOf value
               if index <> -1 
               then
                  let cursor = env.Cursor+index+value.Length
                  let result = String value
                  { env with Cursor=cursor; Result=result; Success=true }
               else { env with Success=false }
            else
               if subject.Substring(env.Cursor).StartsWith(value)
               then
                  let cursor = env.Cursor+value.Length
                  let result = String value
                  { env with Cursor=cursor; Result=result;Success = true }
               else { env with Success = false }
      | Or(patterns) -> 
         let rec findPattern = function
            | [] -> { env with Success = false }
            | p::ps ->
               let newEnv = tryPattern env p
               if newEnv.Success 
               then newEnv
               else findPattern ps
         findPattern patterns
      | ConditionalAssignment(pattern,subject) ->
         let env = tryPattern env pattern
         if env.Success then assign subject env.Result
         env
      | Len(n) ->
         if n > env.Subject.Length + env.Cursor then 
            { env with Result=String ""; Success=false }
         else
            let result = env.Subject.Substring(env.Cursor,n)
            { env with Cursor=env.Cursor+n; Result=String result; Success=true }
      | Rem ->         
         let result = env.Subject.Substring(env.Cursor)
         { env with Result=String result; Cursor=env.Subject.Length-1 }
   let rec nextLine i =
      let line = lines.[i]      
      let env =
         match line.Command with           
         | Assign(subject, expressions) ->             
            let value = 
               match expressions with
               | expression::[] -> evaluate expression
               | _ -> concat expressions
            assign subject value
            { Subject=subject; Result=value; Cursor=0; Success=true}
         | Match(subject, patterns) ->                        
            let subject = concat subject |> toString
            let rec applyPattern env = function
               | [] -> env
               | p::ps ->
                  let newEnv = tryPattern env p
                  if newEnv.Success 
                  then applyPattern newEnv ps
                  else newEnv
            let env = { Subject=subject; Cursor=0; Result=String ""; Success=true}
            applyPattern env patterns
         | Unit -> { Subject=""; Cursor=0; Result=String ""; Success=true }
      match line.Transfer with
      | None -> 
         if i < lines.Length-1 then nextLine (i+1)
      | Some(transfer) ->
         let j = 
            lines 
            |> List.findIndex (fun line -> 
                  match line.Label with 
                  | Some label -> label = transfer.Goto 
                  | None -> false)                        
         match transfer.On, env.Success with
         | Success, true -> nextLine j
         | Failure, false -> nextLine j
         | Any, _ -> nextLine j
         | _ when i < lines.Length-1 -> nextLine (i+1)
         | _ -> ()         
   nextLine 0
// [/snippet]

// [snippet:Hello World Example]
(*
          OUTPUT = "Hello world"
*)
let input () = ""
let output = printfn "%s"
[{Label=None; Command=Assign("OUTPUT",[Value(String("Hello World"))]);Transfer=None}]
|> run input output
// > Hello World
// [/snippet]

// [snippet: Input Example]
(*
          OUTPUT = "What is your name?"
          Username = INPUT
          OUTPUT = "Thank you, " Username
*)
[{Label=None; Command=Assign("OUTPUT",[Value(String("What is your name?"))]);Transfer=None}
 {Label=None; Command=Assign("Username",[Variable("INPUT")]);Transfer=None}
 {Label=None; Command=Assign("OUTPUT",[Value(String("Thank you, "));Variable("Username")]);
                                      Transfer=None}]
|> run (fun () -> "Doctor") output
// > What is your name?
// > Thank you, Doctor
// [/snippet]

// [snippet:Control Flow Example]
(*
          OUTPUT = "What is your name?"
          Username = INPUT
          Username "Jay"                                           :S(LOVE)
          Username "Kay"                                           :S(HATE)
MEH       OUTPUT = "Hi, " Username                                 :(END)
LOVE      OUTPUT = "How nice to meet you, " Username               :(END)
HATE      OUTPUT = "Oh. It's you, " Username
END
*)
let program =
   [
   {Label=None; Command=Assign("OUTPUT",[Value(String("What is your name?"))]);Transfer=None}
   {Label=None; Command=Assign("Username",[Variable("INPUT")]);Transfer=None}
   {Label=None; Command=Match([Variable("Username")],[Expression(Value(String "J"))]);
                Transfer=Some {On=Success;Goto="LOVE"}}
   {Label=None; Command=Match([Variable("Username")],[Expression(Value(String "K"))]);
                Transfer=Some {On=Success;Goto="HATE"}}
   {Label=Some "MEH"; 
    Command=Assign("OUTPUT",[Value(String "Hi, ");Variable("Username")]);
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "LOVE"; 
    Command=Assign("OUTPUT",[Value(String "How nice to meet you, ");Variable("Username")]);
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "HATE"; 
    Command=Assign("OUTPUT",[Value(String "Oh. It's you, ");Variable("Username")]);
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "END"; Command=Unit;Transfer=None}
   ]

program |> run (fun () -> "Jay") output
// > What is your name?
// > How nice to meet you, Jay
program |> run (fun () -> "Kay") output
// > What is your name?
// > Oh. It's you, Kay
program |> run (fun () -> "Bob") output
// > What is your name?
// > Hi, Bob
// [/snippet]

// [snippet:Input Loop Example]
(*
          OUTPUT = "This program will ask you for personal names"
          OUTPUT = "until you press return without giving it one"
          NameCount = 0                                            :(GETINPUT)
AGAIN     NameCount = NameCount + 1
          OUTPUT = "Name " NameCount ": " PersonalName
GETINPUT  OUTPUT = "Please give me name " NameCount + 1 
          PersonalName = INPUT
          PersonalName LEN(1)                                      :S(AGAIN)
          OUTPUT = "Finished. " NameCount " names requested."
END
*)

let loop =
   [
   {Label=None; 
    Command=Assign("OUTPUT",[Value(String("This program will ask you for personal names"))]);
    Transfer=None}
   {Label=None; 
    Command=Assign("OUTPUT",[Value(String("until you press return without giving it one"))]);
    Transfer=None}
   {Label=None; 
    Command=Assign("NameCount",[Value(Integer(0))]);
    Transfer=Some {On=Any;Goto="GETINPUT"}}
   {Label=Some "AGAIN"; 
    Command=Assign("NameCount",[Arithmetic(Variable("NameCount"),Add,Value(Integer(1)))]);
    Transfer=None}
   {Label=None; 
    Command=Assign("OUTPUT",[Value(String("Name ")); Variable("NameCount");
                             Value(String(": "));Variable("PersonalName")]);
    Transfer=None}      
   {Label=Some "GETINPUT"; Command=Assign("PersonalName",[Variable("INPUT")]); Transfer=None}
   {Label=None; 
    Command=Match([Variable("PersonalName")],[Len(1)]); 
    Transfer=Some {On=Success;Goto="AGAIN"}}
   {Label=None; 
    Command=Assign("OUTPUT", [Value(String("Finished. ")); Variable("NameCount");
                              Value(String(" names requested."))])
    Transfer=None}
   ]

let names () =
   let names = seq ["Billy"; "Bob"; "Thornton"]
   let e = names.GetEnumerator()
   fun () -> if e.MoveNext() then e.Current else ""
loop |> run (names ()) output
// > This program will ask you for personal names
// > until you press return without giving it one
// > Name 1: Billy
// > Name 2: Bob
// > Name 3: Thornton
// > Finished. 3 names requested.
// [/snippet]

// [snippet:Simple Pattern Matching Examples]
(*
          'BLUEBIRD' 'BIRD' . OUTPUT
END
*)
let bird =
   [
   {Label=None; 
    Command=Match([Value(String("BLUEBIRD"))],
                  [ConditionalAssignment(Expression(Value(String("BIRD"))),"OUTPUT")]); 
    Transfer=None}
   ]

bird |> run input output
// > BIRD

(*
          B = 'BLUEBIRD'
          B ('BLUE' | 'GOLD') . OUTPUT
          G = 'GOLDFISH'
          B ('BLUE' | 'GOLD') . OUTPUT
          COLOR = 'BLUE' | 'GOLD'
          B COLOR . OUTPUT
END
*)
let color =
   [
   // B = 'BLUEBIRD'
   {Label=None; Command=Assign("B", [Value(String("BLUEBIRD"))]); Transfer=None}
   // B ('BLUE' | 'GOLD') . OUTPUT
   {Label=None; 
    Command=Match([Variable("B")],
                  [ConditionalAssignment(
                     Or [Expression(Value(String("GOLD")));Expression(Value(String("BLUE")))],
                     "OUTPUT")]); 
    Transfer=None}
   // G = 'GOLDFISH'
   {Label=None; Command=Assign("G", [Value(String("GOLDFISH"))]); Transfer=None}
   // B ('BLUE' | 'GOLD') . OUTPUT
   {Label=None; 
    Command=Match([Variable("G")],
                  [ConditionalAssignment(
                     Or [Expression(Value(String("GOLD")));Expression(Value(String("BLUE")))],
                     "OUTPUT")]); 
    Transfer=None}  
   // COLOR = 'BLUE' | 'GOLD'
   {Label=None; 
    Command=Assign("COLOR",
                   [Value(Pattern(Or [Expression(Value(String("GOLD")));
                                      Expression(Value(String("BLUE")))]))]); 
    Transfer=None}
   // B COLOR . OUTPUT
   {Label=None; 
    Command=Match([Variable("B")],
                  [ConditionalAssignment(Expression(Variable("COLOR")), "OUTPUT")]); 
    Transfer=None}  
   ]

color |> run input output
// > BLUE
// > GOLD
// > BLUE

(*
          'THE WINTER WINDS' 'WIN' REM . OUTPUT
*)

let winter = 
   [
   {Label=None; 
    Command=Match([Value(String("'THE WINTER WINDS"))],
                  [Expression(Value(String("WIN")));
                   ConditionalAssignment(Rem,"OUTPUT")]); 
    Transfer=None}
   ]

winter |> run input output
// > TER WINDS
// [/snippet]