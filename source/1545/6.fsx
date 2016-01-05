// [snippet:Abstract Syntax Tree]
type label = string
type name = string
type arithmetic = Add | Subtract | Multiply | Divide | Power
type value = 
   | String of string 
   | Integer of int
   | Array of value[]
   | Table of table
   | Pattern of pattern
and table = System.Collections.Generic.Dictionary<string,value>
and expression = 
   | Value of value
   | Variable of name
   | GetItem of name * expression list
   | Concat of expression list
   | Arithmetic of expression * arithmetic * expression
   | Convert of name * name
   | NewArray of expression
   | NewTable   
and pattern =
   | Expression of expression
   | And of pattern list
   | Or of pattern list
   | ConditionalAssignment of pattern * name
   | ImmediateAssignment of pattern * name
   | Invoke of name * expression list
type transfer =
   | Goto of label
   | OnSuccessOrFailure of label * label
   | OnSuccess of label
   | OnFailure of label
type command =
   | Assign of name * expression
   | SetItem of name * expression list * expression
   | Match of expression * pattern  
   | Replace of name * pattern * expression
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
let (|ToString|_|) = function
   | String s -> Some s 
   | Integer n -> Some (n.ToString())
   | Array _ | Table _ | Pattern _ -> None
let toString = function 
   | ToString s -> s 
   | _ -> invalidOp ""
let (|ParseInt|_|) s =
   match System.Int32.TryParse(s) with
   | true, n -> Some n
   | false, _ -> None
let (|ToInt|_|) = function
   | Integer n -> Some n
   | String(ParseInt n) -> Some n
   | _ -> None
let toInt = function
   | ToInt n -> n  
   | _ -> invalidOp ""
// [/snippet]

// [snippet:Primitive Pattern Functions]
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
   | _ -> failwith ("Function " + name + " not supported")
// [/snippet]

// [snippet:Virtual Machine]
type machine = {
   Variables : System.Collections.Generic.IDictionary<name, value>
   Input : unit -> string
   Output : string -> unit
   }
// [/snippet]

// [snippet:Expression Evaluation]
let rec evaluate machine expression =
   let get name =
      match name with
      | "INPUT" -> machine.Input () |> String
      | _ -> machine.Variables.[name]
   let getItem container index =
      match container with
      | Array(ar) -> 
         let index = toInt index
         if index <= ar.Length then ar.[index-1]
         else nil
      | Table(table) ->          
         match table.TryGetValue(toString index) with
         | true, value -> value
         | false, _ -> nil
      | _ -> invalidOp "Illegal data type"
   match expression with
   | Value(value) -> value
   | Variable(name) -> get name
   | GetItem(name, expressions) ->
      let rec lookup item = function
         | x::xs ->
            let index = evaluate machine x
            let item = getItem item index
            lookup item xs
         | [] -> item            
      let container = get name
      lookup container expressions
   | Concat(expressions) -> concat machine expressions
   | Arithmetic(lhs,op,rhs) ->
      match evaluate machine lhs, evaluate machine rhs with
      | Integer l, Integer r -> Integer(arithmetic op l r)
      | Integer l, String (ParseInt r) -> Integer(arithmetic op l r)
      | String "", Integer r -> Integer(r)
      | String (ParseInt l), Integer r -> Integer(arithmetic op l r)
      | l, r -> invalidOp "Illegal data type"
   | Convert(subject, target) ->
      let value = get subject
      match value, target with
      | Table(table), "ARRAY" ->
         Array 
            [|for kvp in table ->
               Array [|String kvp.Key;String(toString kvp.Value)|] |]
      | _ -> failwith "Not supported"
   | NewArray(expression) ->
      let length = evaluate machine expression |> toInt
      Array(Array.create length nil)
   | NewTable -> Table(table())
and arithmetic op l r =
   match op with
   | Add -> l + r
   | Subtract -> l - r
   | Multiply -> l * r
   | Divide -> l / r
   | Power -> pown l r
and concat machine expressions =
   System.String.Concat 
      [for e in expressions -> evaluate machine e |> toString] 
   |> String
and nil = String ""
// [/snippet]

// [snippet:Pattern Matching]
let assign machine name value =
   match name, value with
   | "OUTPUT", Pattern(_) -> name |> machine.Output
   | "OUTPUT", _ -> value |> toString |> machine.Output
   | _, _ -> ()
   machine.Variables.[name] <- value
let rec tryPattern machine env pattern : environment seq =
   match pattern with
   | Expression(expression) ->
      let subject = env.Subject.Substring(env.Cursor)
      let value = evaluate machine expression
      match value with
      | Pattern pattern -> tryPattern machine env pattern
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
            let newEnvs = tryPattern machine env p       
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
            let newEnvs = tryPattern machine env p
            match newEnvs |> Seq.tryFind (fun env -> env.Success) with
            | Some env -> seq [env] 
            | None -> findPattern ps
      findPattern patterns
   | ConditionalAssignment(pattern,subject) ->
      let envs = tryPattern machine env pattern
      seq {
         for env in envs -> 
            let onSuccess () = assign machine subject env.Result
            { env with Actions=onSuccess::env.Actions }
      }
   | ImmediateAssignment(pattern,subject) ->
      let envs = tryPattern machine env pattern
      seq {
         for env in envs -> 
            assign machine subject env.Result
            env
      }
   | Invoke(name, args) ->
      invoke env name [for arg in args -> evaluate machine arg]
let patternMatch machine subject pattern =      
   let env = { Subject=subject; Cursor=0; Result=String ""; Actions=[]; Success=true }
   let rec tryFromIndex n =
      if n = subject.Length then { env with Success=false }
      else
         let env = { env with Cursor=n }            
         let found = tryPattern machine env pattern |> Seq.tryFind (fun env -> env.Success)
         match found with
         | None -> tryFromIndex (n+1)
         | Some newEnv ->
            for action in newEnv.Actions |> List.rev do action()
            { newEnv with Cursor = env.Cursor }
   tryFromIndex 0
// [/snippet]

// [snippet:Interpereter]
let applyCommand machine command =
   match command with           
   | Assign(subject, expression) ->             
      let value = evaluate machine expression
      assign machine subject value
      value <> nil
   | SetItem(subject, [index], value) ->
      let container = machine.Variables.[subject]
      let index = evaluate machine index
      let value = evaluate machine value
      match container with
      | Array(ar) -> (ar.[(toInt index)-1] <- value)
      | Table(table) -> (table.[toString index] <- value)
      | _ -> invalidOp "Illegal data type"
      true
   | SetItem _ -> failwith "Not supported"
   | Match(subject, pattern) ->
      let subject = evaluate machine subject |> toString                       
      let env = patternMatch machine subject pattern
      env.Success
   | Replace(name, pattern, expression) ->
      let subject = machine.Variables.[name] |> toString
      let env = patternMatch machine subject pattern
      if env.Success then
         let s = subject.Remove(env.Cursor, (env.Result |> toString).Length)
                        .Insert(env.Cursor, evaluate machine expression |> toString)
         assign machine name (String s)
      env.Success
   | Unit -> true
let tryFindTransfer success = function   
   | Goto label -> Some label
   | (OnSuccessOrFailure(label,_) | OnSuccess(label)) when success ->
      Some label
   | (OnSuccessOrFailure(_,label) | OnFailure(label)) when not success ->
      Some label
   | _ -> None
let run (machine:machine) (lines:line list) =   
   let findLabel label =
      lines 
      |> List.findIndex (fun line -> line.Label |> Option.exists ((=)label))
   let rec gotoLine i =
      let line = lines.[i]
      let success = 
         try applyCommand machine line.Command
         with e when line.Transfer.IsSome -> false
      match line.Transfer with
      | None -> 
         if i < lines.Length-1 then gotoLine (i+1)
      | Some(transfer) ->
         match tryFindTransfer success transfer with
         | Some label -> findLabel label |> gotoLine
         | None -> if i < lines.Length-1 then gotoLine (i+1)
   gotoLine 0
// [/snippet]

// [snippet:Internal DSL]
let S s = Value(String s)
let I s = Value(Integer s)
let V s = Variable(s)
let E e = Expression(e)
let P e = Value(Pattern(e))
let (+.) lhs rhs = Arithmetic(lhs,Add,rhs)
let (=.) name expression = Assign(name,expression)
let (.=) pattern name = ConditionalAssignment(pattern,name)
let ($) pattern name = ImmediateAssignment(pattern,name)
let (=?) subject pattern = Match(subject, pattern)
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
let machine = {
   Variables = System.Collections.Generic.Dictionary<_,_>()
   Input = fun () -> ""
   Output = printfn "%s"
}
// [/snippet]
      
// [snippet:Hello World Example]
(*
          OUTPUT = "Hello world"
*)
[Line.Of( "OUTPUT" =. S"Hello World" )]
|> run machine
// > Hello World
// [/snippet]

// [snippet: Input Example]
(*
          OUTPUT = "What is your name?"
          Username = INPUT
          OUTPUT = "Thank you, " Username
*)
[Line.Of( "OUTPUT" =. S"What is your name?")
 Line.Of( "Username" =. V"INPUT")
 Line.Of( "OUTPUT" =. Concat [S"Thank you, "; V"Username"])]
|> run {machine with Input = fun () -> "Doctor"}
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
   Line.Of("OUTPUT" =. S"What is your name?")
   Line.Of("Username" =. V"INPUT")
   Line.Of(V"Username" =? E(S"J"), transfer=OnSuccess "LOVE")
   Line.Of(V"Username" =? E(S"K"), transfer=OnSuccess "HATE")
   Line.Of("OUTPUT" =. Concat [S"Hi, ";V"Username"], label="MEH", 
      transfer=Goto "END")
   Line.Of("OUTPUT" =. Concat [S"How nice to meet you, "; V"Username"], label="LOVE", 
      transfer=Goto "END")
   Line.Of("OUTPUT" =. Concat [S"Oh. It's you, "; V"Username"], label="HATE")
   Line.Of(Unit, label="END")  
   ]
    
program |> run { machine with Input = fun () -> "Jay" }
// > What is your name?
// > How nice to meet you, Jay
program |> run { machine with Input = fun () -> "Kay" }
// > What is your name?
// > Oh. It's you, Kay
program |> run { machine with Input = fun () -> "Bob" }
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
   Line.Of("OUTPUT" =. S"This program will ask you for personal names")
   Line.Of("OUTPUT" =. S"until you press return without giving it one")
   Line.Of("NameCount" =. I 0, transfer=Goto "GETINPUT")
   Line.Of("NameCount" =. V"NameCount" +. I 1, label="AGAIN")
   Line.Of("OUTPUT" =. Concat [S"Name ";V"NameCount";S": ";V"PersonalName"])
   Line.Of("PersonalName" =. V"INPUT", label="GETINPUT")
   Line.Of(V"PersonalName" =? LEN(I 1), transfer=OnSuccess "AGAIN")
   Line.Of("OUTPUT" =. Concat [S"Finished. ";V"NameCount";S" names requested."] )
   ]

let names = seq ["Billy"; "Bob"; "Thornton"]
let successive (inputs:string seq) =
   let e = inputs.GetEnumerator()
   fun () -> if e.MoveNext() then e.Current else ""
loop |> run { machine with Input=successive names }
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
bird |> run machine
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
   Line.Of("B" =. S"BLUEBIRD")
   // B ('BLUE' | 'GOLD') . OUTPUT
   Line.Of(V"B" =? (Or [E(S"BLUE");E(S"GOLD")] .= "OUTPUT"))
   // G = 'GOLDFISH'
   Line.Of("G" =. S"GOLDFISH")
   // B ('BLUE' | 'GOLD') . OUTPUT
   Line.Of(V"G" =? (Or [E(S"BLUE");E(S"GOLD")] .= "OUTPUT"))
   // COLOR = 'BLUE' | 'GOLD'
   Line.Of("COLOR" =. P(Or [E(S"GOLD");E(S"BLUE")]))
   // B COLOR . OUTPUT
   Line.Of(V"B" =? (E(V"COLOR") .= "OUTPUT"))
   ]
color |> run machine
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
b2 |> run machine
// > B
// > 2

(*
          'THE WINTER WINDS' 'WIN' REM . OUTPUT
*)
let winter = [Line.Of(S"THE WINTER WINDS" =? And[E(S"WIN"); REM .= "OUTPUT"])]
winter |> run machine
// > TER WINDS

(*
          'MOUNTAIN' 'O' ARB . OUTPUT 'A'
          'MOUNTAIN' 'O' ARB . OUTPUT 'U'
*)
let mountain =
   [Line.Of(S"MOUNTAIN" =? And[E(S"O");ARB .= "OUTPUT";E(S"A")])
    Line.Of(S"MOUNTAIN" =? And[E(S"O");ARB .= "OUTPUT";E(S"U")])]
mountain |> run machine
// > UNT
// >

(*
          S = 'ABCDA'
          S LEN(3) . OUTPUT RPOS(0)
          S POS(3) LEN(1) . OUTPUT
*)
let abcda =
   [
   Line.Of("S" =. S"ABCDA")
   Line.Of(V"S" =? And[LEN(I 3) .= "OUTPUT";RPOS(I 0)])
   Line.Of(V"S" =? And[POS(I 3);LEN(I 1) .= "OUTPUT"]) 
   ]
abcda |> run machine
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
abcde |> run machine
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
   Line.Of("VOWEL" =. P(ANY(S"AEIOU")))
   Line.Of("DVOWEL" =. P(And[E(V"VOWEL");E(V"VOWEL")]))
   Line.Of("NOTVOWEL" =. P(NOTANY(S"AEIOU")))
   Line.Of(S"VACUUM" =? (E(V"VOWEL") .= "OUTPUT"))
   Line.Of(S"VACUUM" =? (E(V"DVOWEL") .= "OUTPUT"))
   Line.Of(S"VACUUM" =? (And [E(V"VOWEL");E(V"NOTVOWEL")] .= "OUTPUT"))
   ]
vacuum |> run machine
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
   Line.Of("LETTERS" =. S"ABCDEFGHIJKLMNOPQRSTUVWXYZ'-")
   Line.Of(S"SAMPLE LINE" =? (SPAN(V"LETTERS") .= "OUTPUT"))
   Line.Of("WORD" =. P(SPAN(V"LETTERS")))  
   Line.Of("GAP" =. P(BREAK(V"LETTERS")))
   Line.Of("GAPO" =. P(E(V("GAP")) .= "OUTPUT"))
   Line.Of("WORDO" =. P(E(V("WORD")) .= "OUTPUT"))   
   Line.Of(S": ONE, TWO, THREE" =? And [E(V"GAPO");E(V"WORDO");E(V"GAPO");E(V"WORDO")])
   ]

word |> run machine
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
   Line.Of("T" =. S"MASH")
   Line.Of("T" =/ (E(S"M") /= S"B"))
   Line.Of("OUTPUT" =. V"T")
   ]

mash |> run machine
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
   Line.Of("T" =. S"MUCH ADO ABOUT NOTHING")
   Line.Of("T" =/ (E(S("ADO")) /= S"FUSS"))
   Line.Of("OUTPUT" =. V("T"))
   Line.Of("T" =/ (E(S("NOTHING")) /= S""))
   Line.Of("OUTPUT" =. V("T"))
   Line.Of("VOWEL" =. P(ANY(S"AIEOU")))
   Line.Of("T" =/ (E(V"VOWEL") /= S"*"), label="VL", 
      transfer=OnSuccess "VL")
   Line.Of("OUTPUT" =. V("T"))
   ]

much |> run machine
// > MUCH FUSS ABOUT NOTHING
// > MUCH FUSS ABOUT 
// > M*CH F*SS *B**T

(*
*  Define the characters which comprise a 'word'
        WORD   = "'-"  '0123456789' &LCASE

*  Pattern to isolate each word as assign it to ITEM:
        WPAT   = BREAK(WORD) SPAN(WORD) . ITEM

*  Create a table to maintain the word counts
        WCOUNT = TABLE()

*  Read a line of input and obtain the next word
NEXTL   LINE   = REPLACE(INPUT, &UCASE, &LCASE)   :F(DONE)
NEXTW   LINE WPAT =                               :F(NEXTL)

*  Use word as subscript, update its usage count
        WCOUNT<ITEM> = WCOUNT<ITEM> + 1           :(NEXTW)
DONE    A = CONVERT(WCOUNT, 'ARRAY')              :F(EMPTY)

*  Scan array, printing words and counts
        I = 0
PRINT   I = I + 1
        OUTPUT = A<I,1> '--' A<I,2>     :S(PRINT) F(END)

EMPTY   OUTPUT = 'No words'
END
*)

let wordCount =
   [
   Line.Of("LCASE" =. S"abcdefghijklmnopqrstuvwxyz")
   Line.Of("WORD" =. Concat [S"'-";S"0123456789";V"LCASE"])
   Line.Of("WPAT" =. P(And [BREAK(V"WORD");SPAN(V"WORD") .= "ITEM"]))
   Line.Of("WCOUNT" =. NewTable)
   Line.Of("LINE" =. V"INPUT", label="NEXTL", transfer=OnFailure "DONE")
   Line.Of("LINE" =/ (E(V"WPAT") /= S""), label="NEXTW", 
           transfer=OnFailure "NEXTL")
   Line.Of(SetItem("WCOUNT",[V"ITEM"],GetItem("WCOUNT",[V"ITEM"]) +. I 1), 
           transfer=Goto "NEXTW")
   Line.Of("A" =. Convert("WCOUNT", "ARRAY"), label="DONE")
   Line.Of("I" =. I 0)
   Line.Of("I" =. V"I" +. I 1, label="PRINT")
   Line.Of("OUTPUT" =. Concat [GetItem("A",[V"I";I 1]);S"--";GetItem("A",[V"I";I 2])],
           transfer=OnSuccessOrFailure("PRINT", "END"))
   Line.Of("OUTPUT" =. S"No words", label="EMPTY")
   Line.Of(Unit, label="END")
   ]

let lines = seq ["the wind in the willows"]
wordCount |> run { machine with Input = successive lines }
// > the--2
// > wind--1
// > in--1
// > willows--1
// [/snippet]