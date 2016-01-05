// [snippet:Abstract Syntax Tree]
type value = String of string | Integer of int
type label = string
type name = string
type arithmetic = Add | Subtract | Multiply | Divide | Power
type expression = 
   | Literal of value
   | Variable of name
   | Concat of expression list
   | Arithmetic of expression * arithmetic * expression
   | Len of int 
type transferOn = Success | Failure | Any
type transfer = { On:transferOn; Goto:label }
type command =
   | Assign of name * expression
   | Match of expression * expression
   | Unit
type line = {
    Label : label option
    Command : command
    Transfer : transfer option
    }
// [/snippet]
// [snippet:Interpereter]
let toString = function | String s -> s | Integer n -> n.ToString()
let variables = System.Collections.Generic.Dictionary<name, value>()
let (|AsInteger|_|) s =
   match System.Int32.TryParse(s) with
   | true, n -> Some n
   | false, _ -> None
let run input output (lines:line list) =
   let rec evaluate expression =      
      match expression with
      | Literal(value) -> value
      | Variable("INPUT") -> input () |> String
      | Variable(subject) -> variables.[subject]
      | Concat(expressions) ->
         System.String.Concat([for e in expressions -> evaluate e |> toString])
         |> String
      | Arithmetic(lhs,op,rhs) ->
         match evaluate lhs, evaluate rhs with
         | Integer l, Integer r -> Integer(arithmetic op l r)
         | Integer l, String (AsInteger r) -> Integer(arithmetic op l r)
         | _ -> invalidOp "Illegal data type"
      | Len(n) -> invalidOp ""
   and arithmetic op l r =
      match op with
      | Add -> l + r
      | Subtract -> l - r
      | Multiply -> l * r
      | Divide -> l / r
      | Power -> pown l r
   let rec nextLine i =
      let line = lines.[i]
      let result =
         match line.Command with
         | Assign("OUTPUT", expression) ->
            evaluate expression |> toString |> output
            true
         | Assign(subject, expression) ->
            let value = evaluate expression
            variables.[subject] <- value
            true
         | Match(subject, Len(count)) ->
            let s = evaluate subject |> toString
            s.Length >= count
         | Match(subject, pattern) ->
            match evaluate subject, evaluate pattern with
            | String subject, String pattern -> subject.Contains pattern
            | Integer subject, Integer pattern -> subject = pattern
            | _, _ -> invalidOp ""
         | Unit -> true  
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
         match transfer.On, result with
         | Success, true -> nextLine j
         | Failure, false -> nextLine j
         | Any, _ -> nextLine j
         | _ when i < lines.Length-1 -> nextLine (i+1)
         | _ -> ()         
   nextLine 0
// [/snippet]

// [snippet:Hello World]
(*
          OUTPUT = "Hello world"
*)
let input () = ""
let output = printfn "%s"
[{Label=None; Command=Assign("OUTPUT",Literal(String("Hello World")));Transfer=None}]
|> run input output
// > Hello World
// [/snippet]

// [snippet: Input]
(*
          OUTPUT = "What is your name?"
          Username = INPUT
          OUTPUT = "Thank you, " Username
*)
[{Label=None; Command=Assign("OUTPUT",Literal(String("What is your name?")));Transfer=None}
 {Label=None; Command=Assign("Username",Variable("INPUT"));Transfer=None}
 {Label=None; Command=Assign("OUTPUT",Concat [Literal(String("Thank you, "));Variable("Username")]);
                                      Transfer=None}]
|> run (fun () -> "Doctor") output
// > What is your name?
// > Thank you, Doctor
// [/snippet]

// [snippet:Control flow]
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
   {Label=None; Command=Assign("OUTPUT",Literal(String("What is your name?")));Transfer=None}
   {Label=None; Command=Assign("Username",Variable("INPUT"));Transfer=None}
   {Label=None; Command=Match(Variable("Username"),Literal(String "J"));
                Transfer=Some {On=Success;Goto="LOVE"}}
   {Label=None; Command=Match(Variable("Username"),Literal(String "K"));
                Transfer=Some {On=Success;Goto="HATE"}}
   {Label=Some "MEH"; 
    Command=Assign("OUTPUT",Concat [Literal(String "Hi, ");Variable("Username")]);
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "LOVE"; 
    Command=Assign("OUTPUT",Concat [Literal(String "How nice to meet you, ");Variable("Username")]);
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "HATE"; 
    Command=Assign("OUTPUT",Concat [Literal(String "Oh. It's you, ");Variable("Username")]);
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

// [snippet:Input Loop]
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
    Command=Assign("OUTPUT",Literal(String("This program will ask you for personal names")));
    Transfer=None}
   {Label=None; 
    Command=Assign("OUTPUT",Literal(String("until you press return without giving it one")));
    Transfer=None}
   {Label=None; 
    Command=Assign("NameCount",Literal(Integer(0)));
    Transfer=Some {On=Any;Goto="GETINPUT"}}
   {Label=Some "AGAIN"; 
    Command=Assign("NameCount",Arithmetic(Variable("NameCount"),Add,Literal(Integer(1))));
    Transfer=None}
   {Label=None; 
    Command=Assign("OUTPUT",Concat [Literal(String("Name ")); Variable("NameCount");
                                    Literal(String(": "));Variable("PersonalName")]);
    Transfer=None}      
   {Label=Some "GETINPUT"; Command=Assign("PersonalName",Variable("INPUT")); Transfer=None}
   {Label=None; 
    Command=Match(Variable("PersonalName"),Len(1)); 
    Transfer=Some {On=Success;Goto="AGAIN"}}
   {Label=None; 
    Command=Assign("OUTPUT", Concat [Literal(String("Finished. ")); Variable("NameCount");
                                     Literal(String(" names requested."))])
    Transfer=None}
   ]

let names =
   let names = seq ["Billy"; "Bob"; "Thornton"]
   let e = names.GetEnumerator()
   fun () -> if e.MoveNext() then e.Current else ""
loop |> run names output
// > This program will ask you for personal names
// > until you press return without giving it one
// > Name 1: Billy
// > Name 2: Bob
// > Name 3: Thornton
// > Finished. 3 names requested.
// [/snippet]