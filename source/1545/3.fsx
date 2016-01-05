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
   | And of pattern list
   | Or of pattern list
   | ConditionalAssignment of pattern * name
   //----------------------------
   // Primitive Patterns
   | Rem
   | Arb
   // Integer Pattern Functions
   | Len of expression
   | Pos of expression
   | RPos of expression
   | Tab of expression
   | RTab of expression
   // Character Pattern Functions
   | Any of expression
   | NotAny of expression
   | Span of expression
   | Break of expression   
type transferOn = Success | Failure | Goto
type transfer = { On:transferOn; Goto:label }
type command =
   | Assign of name * expression list
   | Match of expression list * pattern
   | Unit
type line = {
    Label : label option
    Command : command
    Transfer : transfer option
    }
// [/snippet]

// [snippet:Environment]
type environment = {
   Subject:string;   
   Cursor:int;
   Actions:(unit->unit) list
   Result:value;
   Success:bool }
// [/snippet]

// [snippet:Built-in Functions]
let rem env =
   let result = env.Subject.Substring(env.Cursor)
   seq [{ env with Result=String result; Cursor=env.Subject.Length-1 }]
let arb env =
   seq {
      for n in 0..env.Subject.Length-env.Cursor ->                    
         let result = env.Subject.Substring(env.Cursor,n)
         {env with Cursor=env.Cursor+n; Result=String result }
   }
let len env n =
   if n > env.Subject.Length + env.Cursor then 
      seq [{ env with Success=false }]
   else
      let result = env.Subject.Substring(env.Cursor,n)
      seq [{ env with Cursor=env.Cursor+n; Result=String result }]
let pos env n =
   if n = env.Cursor then seq [env]
   else seq [{ env with Success=false }]
let rpos env n =
   if n = env.Subject.Length - env.Cursor then seq [env]
   else seq [{ env with Success=false }]
let tab env n =
   if n >= env.Cursor && n < env.Subject.Length then
      let result = env.Subject.Substring(env.Cursor, n - env.Cursor)
      seq [{env with Result=String result; Cursor=n }]
   else seq [{ env with Success=false }]
let rtab env n =
   if (env.Subject.Length - env.Cursor) >= n then
      let length = env.Subject.Length - env.Cursor - n
      let result = env.Subject.Substring(env.Cursor, length)
      seq [{env with Result=String result; Cursor=env.Subject.Length-n }]
   else seq [{ env with Success=false }]
let any env (s:string) =  
   let c = env.Subject.Chars(env.Cursor)   
   if s |> String.exists ((=)c) then
      seq [{ env with Cursor=env.Cursor+1; Result=String(c.ToString()) }]
   else
      seq [{ env with Success=false }]
let notany env (s:string) =
   let c = env.Subject.Chars(env.Cursor)   
   if not(s |> String.exists ((=)c)) then
      seq [{ env with Cursor=env.Cursor+1; Result=String(c.ToString()) }]
   else
      seq [{ env with Success=false }]
let span env (s:string) =    
   let mutable n = 0
   while let c = env.Subject.Chars(env.Cursor+n) in 
         (s |> String.exists ((=)(c))) do n <- n + 1
   if n > 0 
   then
      let result = env.Subject.Substring(env.Cursor,n)
      seq [{ env with Cursor=env.Cursor+n; Result=String result }]
   else seq [{ env with Success=false }]
let ``break`` env s =
   let mutable n = 0
   while let c = env.Subject.Chars(env.Cursor+n) in 
         (s |> String.exists ((<>)(c))) do n <- n + 1  
   let result = env.Subject.Substring(env.Cursor,n)
   seq [{ env with Cursor=env.Cursor+n; Result=String result }]   
// [/snippet]

// [snippet:Interpereter]
let (|AsInt|_|) s =
   match System.Int32.TryParse(s) with
   | true, n -> Some n
   | false, _ -> None
let toString = function 
   | String s -> s 
   | Integer n -> n.ToString()
   | Pattern _ -> invalidOp ""
let toInt = function
   | Integer n -> n
   | String(AsInt n) -> n
   | _ -> invalidOp ""
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
         | Integer l, String (AsInt r) -> Integer(arithmetic op l r)
         | _ -> invalidOp "Illegal data type"
   and arithmetic op l r =
      match op with
      | Add -> l + r
      | Subtract -> l - r
      | Multiply -> l * r
      | Divide -> l / r
      | Power -> pown l r
   and tryPattern (env:environment) pattern : environment seq =
      match pattern with
      | Expression(expression) ->
         let subject = env.Subject.Substring(env.Cursor)
         let value = evaluate expression
         match value with
         | Pattern pattern -> tryPattern env pattern
         | value ->  
            let value = value |> toString
            if subject.StartsWith(value)
            then
               let cursor = env.Cursor+value.Length
               let result = String value
               seq [{ env with Cursor=cursor; Result=result }]
            else 
               seq [{ env with Success = false }]
      | And(patterns) ->
         let rec applyPattern env = function
            | [] -> env
            | p::ps ->
               let newEnvs = tryPattern env p       
               let found =
                  newEnvs |> Seq.tryPick (fun newEnv ->
                     if newEnv.Success then                        
                        let env = applyPattern newEnv ps
                        if env.Success
                        then Some env
                        else None
                     else None
                  )
               match found with
               | Some newEnv -> 
                  let result = toString env.Result + toString newEnv.Result
                  { newEnv with Result = String result }
               | None -> {env with Success=false}
         seq [applyPattern env patterns]
      | Or(patterns) -> 
         let rec findPattern = function
            | [] -> seq [{ env with Success = false }]
            | p::ps ->
               let newEnvs = tryPattern env p
               match newEnvs |> Seq.tryFind (fun env -> env.Success) with
               | Some env -> seq [env] 
               | None -> findPattern ps
         findPattern patterns
      | ConditionalAssignment(pattern,subject) ->
         let envs = tryPattern env pattern
         seq {
            for env in envs -> 
               let onSuccess () = assign subject env.Result
               { env with Actions=onSuccess::env.Actions }
         }     
      // Built-in functions
      | Rem -> rem env
      | Arb -> arb env
      | Len(n) -> len env (evaluate n |> toInt)
      | Pos(n) -> pos env (evaluate n |> toInt)
      | RPos(n) -> rpos env (evaluate n |> toInt)
      | Tab(n) -> tab env (evaluate n |> toInt)
      | RTab(n) -> rtab env (evaluate n |> toInt)
      | Any(s) -> any env (evaluate s |> toString)
      | NotAny(s) -> notany env (evaluate s |> toString)
      | Span(s) -> span env (evaluate s |> toString)
      | Break(s) -> ``break`` env (evaluate s |> toString)
   let rec gotoLine i =
      let line = lines.[i]      
      let success =
         match line.Command with           
         | Assign(subject, expressions) ->             
            let value = 
               match expressions with
               | expression::[] -> evaluate expression
               | _ -> concat expressions
            assign subject value
            true
         | Match(subject, pattern) ->                        
            let subject = concat subject |> toString
            let env = { Subject=subject; Cursor=0; Result=String ""; Actions=[]; Success=true}
            let rec tryFromIndex n =
               if n = subject.Length then false
               else
                  let env = { env with Cursor=n }            
                  let found = tryPattern env pattern |> Seq.tryFind (fun env -> env.Success)
                  match found with
                  | None -> tryFromIndex (n+1)
                  | Some env ->
                     for action in env.Actions |> List.rev do action()
                     true                  
            tryFromIndex 0          
         | Unit -> true
      match line.Transfer with
      | None -> 
         if i < lines.Length-1 then gotoLine (i+1)
      | Some(transfer) ->
         let j = 
            lines 
            |> List.findIndex (fun line -> 
                  match line.Label with 
                  | Some label -> label = transfer.Goto 
                  | None -> false)                        
         match transfer.On, success with
         | Success, true -> gotoLine j
         | Failure, false -> gotoLine j
         | Goto, _ -> gotoLine j
         | _ when i < lines.Length-1 -> gotoLine (i+1)
         | _ -> ()         
   gotoLine 0
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
   {Label=None; Command=Match([Variable("Username")], Expression(Value(String "J")));
                Transfer=Some {On=Success;Goto="LOVE"}}
   {Label=None; Command=Match([Variable("Username")], Expression(Value(String "K")));
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
    Transfer=Some {On=Goto;Goto="GETINPUT"}}
   {Label=Some "AGAIN"; 
    Command=Assign("NameCount",[Arithmetic(Variable("NameCount"),Add,Value(Integer(1)))]);
    Transfer=None}
   {Label=None; 
    Command=Assign("OUTPUT",[Value(String("Name ")); Variable("NameCount");
                             Value(String(": "));Variable("PersonalName")]);
    Transfer=None}      
   {Label=Some "GETINPUT"; Command=Assign("PersonalName",[Variable("INPUT")]); Transfer=None}
   {Label=None; 
    Command=Match([Variable("PersonalName")],Len(Value(Integer(1)))); 
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
                   ConditionalAssignment(
                     Expression(Value(String("BIRD"))),
                     "OUTPUT")); 
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
                  ConditionalAssignment(
                     Or [Expression(Value(String("GOLD")));Expression(Value(String("BLUE")))],
                     "OUTPUT")); 
    Transfer=None}
   // G = 'GOLDFISH'
   {Label=None; Command=Assign("G", [Value(String("GOLDFISH"))]); Transfer=None}
   // B ('BLUE' | 'GOLD') . OUTPUT
   {Label=None; 
    Command=Match([Variable("G")],
                  ConditionalAssignment(
                     Or [Expression(Value(String("GOLD")));Expression(Value(String("BLUE")))],
                     "OUTPUT")); 
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
                  ConditionalAssignment(Expression(Variable("COLOR")), "OUTPUT")); 
    Transfer=None}  
   ]

color |> run input output
// > BLUE
// > GOLD
// > BLUE

(*
          'B2' ('A' | 'B') . OUTPUT (1 | 2 | 3) . OUTPUT
*)
let b2 = 
   [
   {Label=None; 
    Command=Match([Value(String("B2"))],
                  And [
                     ConditionalAssignment(
                        Or[Expression(Value(String "A"));
                           Expression(Value(String "B"))],
                        "OUTPUT")
                     ConditionalAssignment(
                        Or[Expression(Value(String "1"));
                           Expression(Value(String "2"));
                           Expression(Value(String "3"))],
                        "OUTPUT")
                  ]); 
    Transfer=None}
   ]

b2 |> run input output
// > B
// > 2

(*
          'THE WINTER WINDS' 'WIN' REM . OUTPUT
*)
let winter = 
   [
   {Label=None; 
    Command=Match([Value(String("THE WINTER WINDS"))],
                  And [Expression(Value(String("WIN")));
                       ConditionalAssignment(Rem,"OUTPUT")]); 
    Transfer=None}
   ]

winter |> run input output
// > TER WINDS

(*
          'MOUNTAIN' 'O' ARB . OUTPUT 'A'
          'MOUNTAIN' 'O' ARB . OUTPUT 'U'
*)
let mountain = 
   [
   {Label=None; 
    Command=Match([Value(String("MOUNTAIN"))],
                  And [Expression(Value(String("O")));
                       ConditionalAssignment(Arb,"OUTPUT");
                       Expression(Value(String("A")))]); 
    Transfer=None}
   {Label=None; 
    Command=Match([Value(String("MOUNTAIN"))],
                  And [Expression(Value(String("O")));
                       ConditionalAssignment(Arb,"OUTPUT");
                       Expression(Value(String("U")))]); 
    Transfer=None}
   ]

mountain |> run input output
// > UNT
// >

(*
          S = 'ABCDA'
          S LEN(3) . OUTPUT RPOS(0)
          S POS(3) LEN(1) . OUTPUT
*)
let abcda =
   [
   {Label=None; Command=Assign("S", [Value(String("ABCDA"))]); Transfer=None}
   {Label=None; 
    Command=Match([Variable("S")],
                  And [ConditionalAssignment(Len(Value(Integer(3))),"OUTPUT");
                       RPos(Value(Integer(0)))]); 
    Transfer=None}
   {Label=None; 
    Command=Match([Variable("S")],
                  And [Pos(Value(Integer(3)))
                       ConditionalAssignment(Len(Value(Integer(1))),"OUTPUT")]); 
    Transfer=None}
   ]

abcda |> run input output
// > CDA
// > D

(*
          'ABCDE' TAB(2) . OUTPUT
          'ABCDE' TAB(2) . OUTPUT RTAB(1) . OUTPUT REM . OUTPUT
*)

let abcde =
   [
   {Label=None; 
    Command=Match([Value(String("ABCDE"))], 
                   And(
                     [ConditionalAssignment(Tab(Value(Integer(2))),"OUTPUT");
                      ConditionalAssignment(RTab(Value(Integer(1))),"OUTPUT");
                      ConditionalAssignment(Rem,"OUTPUT")]));
    Transfer=None}    
   ]

abcde |> run input output
// > AB
// > CD
// > E

(*
          VOWEL = ANY('AEIOU')          
          DVOWEL = VOWEL VOWEL
          NOTVOWEL = NOTANY('AEIOU')
          'VACUUM' VOWEL . OUTPUT
          'VACUUM' DVOWEL . OUTPUT
          'VACUUM' (VOWEL NOTVOWEL) . OUTPUT
*)

let vacuum =
   [
   {Label=None; Command=Assign("VOWELS", [Value(String("AEIOU"))]); Transfer=None}
   {Label=None; 
    Command=Assign("VOWEL", [Value(Pattern(Any(Variable("VOWELS"))))]); 
    Transfer=None}
   {Label=None; 
    Command=Assign("DVOWEL", 
                   [Value(
                     Pattern(
                        And [Expression(Variable("VOWEL"));
                             Expression(Variable("VOWEL"))])
                    )]);                                       
    Transfer=None}
   {Label=None; 
    Command=Assign("NOTVOWEL", [Value(Pattern(NotAny(Variable("VOWELS"))))]); 
    Transfer=None}
   {Label=None; 
    Command=Match([Value(String("VACUUM"))],
                  ConditionalAssignment(Expression(Variable("VOWEL")),"OUTPUT"));
    Transfer=None}
   {Label=None; 
    Command=Match([Value(String("VACUUM"))],
                  ConditionalAssignment(Expression(Variable("DVOWEL")),"OUTPUT"));
    Transfer=None}
   {Label=None; 
    Command=Match([Value(String("VACUUM"))],
                  ConditionalAssignment(
                     And [Expression(Variable("VOWEL"));
                          Expression(Variable("NOTVOWEL"))],
                     "OUTPUT"));                       
    Transfer=None}
   ]

vacuum |> run input output
// > A
// > UU
// > AC

(*
          LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ'-"
          WORD = SPAN(LETTERS)
          GAP = BREAK(LETTERS)
          'SAMPLE LINE' WORD . OUTPUT
*)

let word =
   [
   {Label=None; 
    Command=Assign("LETTERS", [Value(String("ABCDEFGHIJKLMNOPQRSTUVWXYZ'-"))]); 
    Transfer=None}
   {Label=None; 
    Command=Match([Value(String("SAMPLE LINE"))],                    
                  ConditionalAssignment(Span(Variable("LETTERS")),"OUTPUT"));
    Transfer=None}    
   ]

word |> run input output
// > SAMPLE
// [/snippet]