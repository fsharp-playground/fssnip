// [snippet:Abstract Syntax Tree]
type value = String of string | Integer of int
type label = string
type subject = string
type expression = 
   | Literal of value
   | Variable of subject
   | Concat of expression * expression
type transferOn = Success | Failure | Any
type transfer = { On:transferOn; Goto:label }
type command =
   | Assign of subject * expression
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
let variables = System.Collections.Generic.Dictionary<subject, value>()
let run input output (lines:line list) =
   let rec evaluate expression =      
      match expression with
      | Literal(value) -> value
      | Variable("INPUT") -> input () |> String
      | Variable(subject) -> variables.[subject]
      | Concat(lhs,rhs) ->
         match evaluate lhs, evaluate rhs with
         | String l, String r -> String(l+r)
         | Integer l, Integer r -> Integer(l+r)
         | _ -> invalidOp ""
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
         | Match(lhs, rhs) ->           
            evaluate lhs = evaluate rhs 
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
 {Label=None; Command=Assign("OUTPUT",Concat(Literal(String("Thank you, ")),Variable("Username")));
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
   {Label=None; Command=Match(Variable("Username"),Literal(String "Jay"));
                Transfer=Some {On=Success;Goto="LOVE"}}
   {Label=None; Command=Match(Variable("Username"),Literal(String "Kay"));
                Transfer=Some {On=Success;Goto="HATE"}}
   {Label=Some "MEH"; 
    Command=Assign("OUTPUT",Concat(Literal(String "Hi, "),Variable("Username")));
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "LOVE"; 
    Command=Assign("OUTPUT",Concat(Literal(String "How nice to meet you, "),Variable("Username")));
    Transfer=Some {On=Success;Goto="END"}}
   {Label=Some "HATE"; 
    Command=Assign("OUTPUT",Concat(Literal(String "Oh. It's you, "),Variable("Username")));
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