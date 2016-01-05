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
   | Invoke of name * expression list   
type transferOn = Success | Failure | Goto
type transfer = { On:transferOn; Goto:label }
type command =
   | Assign of name * expression list
   | Match of expression list * pattern  
   | Replace of name * pattern * expression list
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

// [snippet:Conversion Functions]
let (|AsInt|_|) s =
   match System.Int32.TryParse(s) with
   | true, n -> Some n
   | false, _ -> None
let toString = function 
   | String s -> s 
   | Integer n -> n.ToString()
   | Pattern _ -> invalidOp ""
let (|ToString|_|) = function
   | String s -> Some s 
   | Integer n -> Some (n.ToString())
   | Pattern _ -> None
let toInt = function
   | Integer n -> n
   | String(AsInt n) -> n
   | _ -> invalidOp ""
let (|ToInt|_|) = function
   | Integer n -> Some n
   | String(AsInt n) -> Some n
   | _ -> None
// [/snippet]

// [snippet:Built-in Functions]
/// Match remainder of subject
let rem env =
   let result = env.Subject.Substring(env.Cursor)
   seq [{ env with Result=String result; Cursor=env.Subject.Length-1 }]
///  Match arbitrary characters
let arb env =
   seq {
      for n in 0..env.Subject.Length-env.Cursor ->                    
         let result = env.Subject.Substring(env.Cursor,n)
         {env with Cursor=env.Cursor+n; Result=String result }
   }
/// Match fixed-length string
let len env n =
   if n > env.Subject.Length + env.Cursor then 
      seq [{ env with Success=false }]
   else
      let result = env.Subject.Substring(env.Cursor,n)
      seq [{ env with Cursor=env.Cursor+n; Result=String result }]
/// Verify cursor position
let pos env n =
   if n = env.Cursor then seq [env]
   else seq [{ env with Success=false }]
let rpos env n =
   if n = env.Subject.Length - env.Cursor then seq [env]
   else seq [{ env with Success=false }]
/// Match to fixed position
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
/// Match one character
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
/// Match a run of characters
let span env (s:string) =    
   let mutable n = 0  
   while env.Cursor+n < env.Subject.Length &&  
         let c = env.Subject.Chars(env.Cursor+n) in
         (s |> String.exists ((=)(c))) do n <- n + 1
   if n > 0 
   then
      let result = env.Subject.Substring(env.Cursor,n)
      seq [{ env with Cursor=env.Cursor+n; Result=String result }]
   else seq [{ env with Success=false }]
let ``break`` env s =   
   let mutable n = 0
   while env.Cursor+n < env.Subject.Length &&
         let c = env.Subject.Chars(env.Cursor+n) in 
         (s |> String.exists ((=)(c)) |> not) do n <- n + 1
   let result = env.Subject.Substring(env.Cursor,n)
   seq [{ env with Cursor=env.Cursor+n; Result=String result }]
// Invoke a built-in function
let invoke env name args =
   match name, args with
   | "REM", [] -> rem env 
   | "ARB", [] -> arb env
   | "LEN", [ToInt n] -> len env n
   | "POS", [ToInt n] -> pos env n
   | "RPOS", [ToInt n] -> rpos env n
   | "TAB", [ToInt n] -> tab env n
   | "RTAB", [ToInt n] -> rtab env n
   | "ANY", [ToString s] -> any env s
   | "NOTANY", [ToString s] -> notany env s
   | "SPAN", [ToString s] -> span env s
   | "BREAK", [ToString s] -> ``break`` env s
   | _ -> failwith "Not supported"
// [/snippet]

// [snippet:Interpereter]
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
      | Invoke(name, args) ->
         invoke env name [for arg in args -> evaluate arg]
   let patternMatch subject pattern =      
      let env = { Subject=subject; Cursor=0; Result=String ""; Actions=[]; Success=true}
      let rec tryFromIndex n =
         if n = subject.Length then { env with Success=false }
         else
            let env = { env with Cursor=n }            
            let found = tryPattern env pattern |> Seq.tryFind (fun env -> env.Success)
            match found with
            | None -> tryFromIndex (n+1)
            | Some newEnv ->
               for action in newEnv.Actions |> List.rev do action()
               { newEnv with Cursor = env.Cursor }
      tryFromIndex 0
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
            let env = patternMatch subject pattern
            env.Success
         | Replace(name, pattern, expressions) ->
            let subject = variables.[name] |> toString
            let env = patternMatch subject pattern
            if env.Success then
               let subject = subject.Remove(env.Cursor, (env.Result |> toString).Length)
               let subject = subject.Insert(env.Cursor, concat expressions |> toString)
               variables.[name] <- String subject
            env.Success
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

// [snippet:Internal DSL]
let S s = Value(String s)
let I s = Value(Integer s)
let V s = Variable(s)
let E e = Expression(e)
let P e = Value(Pattern(e))
let (.+) lhs rhs = Arithmetic(lhs,Add,rhs)
let (=.) name expressions = Assign(name,expressions)
let (.=) pattern name = ConditionalAssignment(pattern,name)
let (=?) subject pattern = Match([subject], pattern)
let (/=) pattern expressions = (pattern,expressions)
let (=/) (name) (pattern,expressions) = Replace(name,pattern,expressions)
let ARB = Invoke("ARB",[])
let REM = Invoke("REM",[])
let LEN(e) = Invoke("LEN",[e])
let POS(e) = Invoke("POS",[e])
let RPOS(e) = Invoke("RPOS",[e])
let TAB(e) = Invoke("TAB",[e])
let RTAB(e) = Invoke("RTAB",[e])
let ANY(e) = Invoke("ANY",[e])
let NOTANY(e) = Invoke("NOTANY",[e])
let SPAN(e) = Invoke("SPAN",[e])
let BREAK(e) = Invoke("BREAK",[e])
type Line =
   static member Of(command,?label,?transfer) =
      {Command=command; Label=label; Transfer=transfer}
// [/snippet]
      
// [snippet:Hello World Example]
(*
          OUTPUT = "Hello world"
*)
let input () = ""
let output = printfn "%s"
[Line.Of( "OUTPUT" =. [S"Hello World"] )]
|> run input output
// > Hello World
// [/snippet]


// [snippet: Input Example]
(*
          OUTPUT = "What is your name?"
          Username = INPUT
          OUTPUT = "Thank you, " Username
*)
[Line.Of( "OUTPUT" =. [S"What is your name?"])
 Line.Of( "Username" =. [V"INPUT"])
 Line.Of( "OUTPUT" =. [S"Thank you, "; V"Username"])]
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
   Line.Of("OUTPUT" =. [S"What is your name?"])
   Line.Of("Username" =. [V"INPUT"])
   Line.Of(V"Username" =? E(S"J"), transfer={On=Success;Goto="LOVE"})
   Line.Of(V"Username" =? E(S"K"), transfer={On=Success;Goto="HATE"})
   Line.Of("OUTPUT" =. [S"Hi, ";V"Username"], label="MEH", 
      transfer={On=Goto;Goto="END"})
   Line.Of("OUTPUT" =. [S"How nice to meet you, "; V"Username"], label="LOVE", 
      transfer={On=Goto;Goto="END"})
   Line.Of("OUTPUT" =. [S"Oh. It's you, "; V"Username"], label="HATE")
   Line.Of(Unit, label="END")  
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
   Line.Of("OUTPUT" =. [S"This program will ask you for personal names"])
   Line.Of("OUTPUT" =. [S"until you press return without giving it one"])
   Line.Of("NameCount" =. [I 0], transfer={On=Goto;Goto="GETINPUT"})
   Line.Of("NameCount" =. [V"NameCount" .+ I 1], label="AGAIN")
   Line.Of("OUTPUT" =. [S"Name ";V"NameCount";S": ";V"PersonalName"])
   Line.Of("PersonalName" =. [V"INPUT"], label="GETINPUT")
   Line.Of(V"PersonalName" =? LEN(I 1), transfer={On=Success;Goto="AGAIN"})
   Line.Of("OUTPUT" =. [S"Finished. ";V"NameCount";S" names requested."] )
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
let bird = [Line.Of( S"BLUEBIRD" =? (E(S"BIRD") .= "OUTPUT") )]
bird |> run input output
// > BIRD

(*
          B = 'BLUEBIRD'
          B ('BLUE' | 'GOLD') . OUTPUT
          G = 'GOLDFISH'
          G ('BLUE' | 'GOLD') . OUTPUT
          COLOR = 'BLUE' | 'GOLD'
          B COLOR . OUTPUT
END
*)
let color =
   [
   // B = 'BLUEBIRD'
   Line.Of("B" =. [S"BLUEBIRD"])
   // B ('BLUE' | 'GOLD') . OUTPUT
   Line.Of(V"B" =? (Or [E(S"BLUE");E(S"GOLD")] .= "OUTPUT"))
   // G = 'GOLDFISH'
   Line.Of("G" =. [S"GOLDFISH"])
   // B ('BLUE' | 'GOLD') . OUTPUT
   Line.Of(V"G" =? (Or [E(S"BLUE");E(S"GOLD")] .= "OUTPUT"))
   // COLOR = 'BLUE' | 'GOLD'
   Line.Of("COLOR" =. [P(Or [E(S"GOLD");E(S"BLUE")])])
   // B COLOR . OUTPUT
   Line.Of(V"B" =? (E(V"COLOR") .= "OUTPUT"))
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
   Line.Of(S"B2" =? And [ Or [E(S"A");E(S"B")] .= "OUTPUT"
                          Or [E(S"1");E(S"2");E(S"3")] .= "OUTPUT"] )
   ]
b2 |> run input output
// > B
// > 2

(*
          'THE WINTER WINDS' 'WIN' REM . OUTPUT
*)
let winter = [Line.Of(S"THE WINTER WINDS" =? And[E(S"WIN"); REM .= "OUTPUT"])]
winter |> run input output
// > TER WINDS

(*
          'MOUNTAIN' 'O' ARB . OUTPUT 'A'
          'MOUNTAIN' 'O' ARB . OUTPUT 'U'
*)
let mountain =
   [Line.Of(S"MOUNTAIN" =? And[E(S"O");ARB .= "OUTPUT";E(S"A")])
    Line.Of(S"MOUNTAIN" =? And[E(S"O");ARB .= "OUTPUT";E(S"U")])]
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
   Line.Of("S" =. [S"ABCDA"])
   Line.Of(V"S" =? And[LEN(I 3) .= "OUTPUT";RPOS(I 0)])
   Line.Of(V"S" =? And[POS(I 3);LEN(I 1) .= "OUTPUT"]) 
   ]
abcda |> run input output
// > CDA
// > D

(*
          'ABCDE' TAB(2) . OUTPUT RTAB(1) . OUTPUT REM . OUTPUT
*)
let abcde =
   [
   Line.Of(S"ABCDE" =? And [TAB(I 2) .= "OUTPUT"
                            RTAB(I 1) .= "OUTPUT"
                            REM .= "OUTPUT" ])
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
   Line.Of("VOWEL" =. [P(ANY(S"AEIOU"))])
   Line.Of("DVOWEL" =. [P(And[E(V"VOWEL");E(V"VOWEL")])])
   Line.Of("NOTVOWEL" =. [P(NOTANY(S"AEIOU"))])
   Line.Of(S"VACUUM" =? (E(V"VOWEL") .= "OUTPUT"))
   Line.Of(S"VACUUM" =? (E(V"DVOWEL") .= "OUTPUT"))
   Line.Of(S"VACUUM" =? (And [E(V"VOWEL");E(V"NOTVOWEL")] .= "OUTPUT"))
   ]
vacuum |> run input output
// > A
// > UU
// > AC

(*
          LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ'-"
          WORD = SPAN(LETTERS)
          'SAMPLE LINE' WORD . OUTPUT
          GAP = BREAK(LETTERS)
          GAPO = GAP . OUTPUT
          WORDO = WORD . OUTPUT
          ': ONE, TWO, THREE' GAPO WORDO GAPO WORDO
*)

let word =
   [
   Line.Of("LETTERS" =. [S"ABCDEFGHIJKLMNOPQRSTUVWXYZ'-"])
   Line.Of(S"SAMPLE LINE" =? (SPAN(V"LETTERS") .= "OUTPUT"))
   Line.Of("WORD" =. [P(SPAN(V"LETTERS"))])  
   Line.Of("GAP" =. [P(BREAK(V"LETTERS"))])  
   Line.Of("GAPO" =. [P(E(V("GAP")) .= "OUTPUT")])
   Line.Of("WORDO" =. [P(E(V("WORD")) .= "OUTPUT")])   
   Line.Of(S": ONE, TWO, THREE" =? And [E(V"GAPO");E(V"WORDO");E(V"GAPO");E(V"WORDO")])
   ]

word |> run input output
// > SAMPLE
// > : 
// > ONE
// > , 
// > TWO

(*
          T = "MASH"
          T 'M' = 'B'
          OUTPUT = T
*)

let mash =
   [
   Line.Of("T" =. [S"MASH"])
   Line.Of("T" =/ (E(S"M") /= [S"B"]))
   Line.Of("OUTPUT" =. [V"T"])
   ]

mash |> run input output
// > BASH

(*
          T = 'MUCH ADO ABOUT NOTHING'
          T 'ADO' = 'FUSS'
          OUTPUT = T
          T 'NOTHING' =
          OUTPUT = T
          VOWEL = ANY('AEIOU')
 VL       T VOWEL = '*'          :S(VL)        
          OUTPUT = T
*)

let much =
   [
   Line.Of("T" =. [S"MUCH ADO ABOUT NOTHING"])
   Line.Of("T" =/ (E(S("ADO")) /= [S"FUSS"]))
   Line.Of("OUTPUT" =. [V("T")])
   Line.Of("T" =/ (E(S("NOTHING")) /= [S""]))
   Line.Of("OUTPUT" =. [V("T")])
   Line.Of("VOWEL" =. [P(ANY(S"AIEOU"))])
   Line.Of("T" =/ (E(V"VOWEL") /= [S"*"]), label="VL", 
      transfer={On=Success;Goto="VL"})
   Line.Of("OUTPUT" =. [V("T")])
   ]

much |> run input output
// > MUCH FUSS ABOUT NOTHING
// > MUCH FUSS ABOUT 
// > M*CH F*SS *B**T 
// [/snippet]